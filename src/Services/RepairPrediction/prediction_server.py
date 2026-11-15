"""Serve the trained repair models over local gRPC."""

import argparse
import math
import os
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime
from pathlib import Path

import grpc
import joblib

import repair_prediction_pb2 as messages
import repair_prediction_pb2_grpc as rpc
from dataset import make_features


DEFAULT_MODELS = Path(__file__).resolve().parents[3] / "artifacts/ml"


def request_features(request):
    try:
        appointment = datetime.fromisoformat(request.appointment_local)
    except ValueError as error:
        raise ValueError("appointment_local must be a local ISO 8601 date and time") from error
    if appointment.tzinfo is not None or "T" not in request.appointment_local:
        raise ValueError("appointment_local must include a time and no UTC offset")

    mechanic = request.mechanic_id.strip()
    if not mechanic or not 1886 <= request.vehicle_year <= appointment.year:
        raise ValueError("mechanic_id and a valid vehicle_year are required")
    if not request.HasField("mileage_km") or request.mileage_km < 0:
        raise ValueError("mileage_km must be supplied and non-negative")

    services = request.service_ids
    if (not services or any(service <= 0 for service in services)
            or len(set(services)) != len(services) or request.service_count < len(services)):
        raise ValueError("service_ids must be distinct positive IDs and service_count must cover all services")

    budget = request.planned_budget_minutes if request.HasField("planned_budget_minutes") else None
    if budget is not None and (not math.isfinite(budget) or budget <= 0):
        raise ValueError("planned_budget_minutes must be positive when supplied")

    features = make_features(appointment, mechanic, request.vehicle_year, request.mileage_km,
                             services, request.service_count)
    return features, budget


class RepairPrediction(rpc.RepairPredictionServicer):
    def __init__(self, model_dir):
        # Load only trusted, locally trained artifacts. Requests cannot choose model paths.
        duration_path = model_dir / "duration/duration.joblib"
        delay_path = model_dir / "delay/delay.joblib"
        for path in (duration_path, delay_path):
            if not path.is_file():
                raise FileNotFoundError(f"Missing model: {path}. Run the corresponding training script first.")
        self.duration = joblib.load(duration_path)
        self.delay = joblib.load(delay_path)
        self.delay_class_index = list(self.delay.classes_).index(1)

    def PredictRepair(self, request, context):
        try:
            features, budget = request_features(request)
        except ValueError as error:
            context.abort(grpc.StatusCode.INVALID_ARGUMENT, str(error))

        duration = float(self.duration.predict([features])[0])
        if not math.isfinite(duration):
            context.abort(grpc.StatusCode.INTERNAL, "Duration model returned a non-finite estimate")
        reply = messages.PredictRepairReply(estimated_duration_minutes=max(0.0, duration))
        if budget is not None:
            delay_features = {**features, "planned_budget_minutes": budget}
            probability = float(self.delay.predict_proba([delay_features])[0, self.delay_class_index])
            if not 0.0 <= probability <= 1.0:
                context.abort(grpc.StatusCode.INTERNAL, "Delay model returned an invalid probability")
            reply.delay_probability = probability
        return reply


def start_server(model_dir, port):
    service = RepairPrediction(model_dir)
    server = grpc.server(ThreadPoolExecutor(max_workers=4))
    rpc.add_RepairPredictionServicer_to_server(service, server)
    bound_port = server.add_insecure_port(f"127.0.0.1:{port}")
    server.start()
    return server, bound_port


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--model-dir", type=Path, default=DEFAULT_MODELS)
    parser.add_argument("--port", type=int, default=os.environ.get("REPAIR_PREDICTION_PORT", "50051"))
    args = parser.parse_args()
    if not 0 <= args.port <= 65535:
        parser.error("--port must be between 0 and 65535")

    try:
        server, port = start_server(args.model_dir, args.port)
    except (FileNotFoundError, RuntimeError) as error:
        parser.exit(1, f"Cannot start RepairPrediction: {error}\n")
    print(f"RepairPrediction gRPC listening on 127.0.0.1:{port}", flush=True)
    try:
        server.wait_for_termination()
    except KeyboardInterrupt:
        server.stop(grace=2).wait()


if __name__ == "__main__":
    main()
