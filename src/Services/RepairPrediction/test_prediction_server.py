"""Local gRPC integration tests using the trained artifacts."""

import math
import unittest

import grpc

import repair_prediction_pb2 as messages
from prediction_server import DEFAULT_MODELS, start_server
from repair_prediction_pb2_grpc import RepairPredictionStub


class PredictionServerTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        server, port = start_server(DEFAULT_MODELS, 0)
        cls.addClassCleanup(lambda: server.stop(grace=0).wait())
        channel = grpc.insecure_channel(f"127.0.0.1:{port}")
        cls.addClassCleanup(channel.close)
        cls.stub = RepairPredictionStub(channel)

    def setUp(self):
        self.request = messages.PredictRepairRequest(
            appointment_local="2026-10-01T10:00:00",
            mechanic_id="smoke-test-mechanic",
            vehicle_year=2018,
            mileage_km=85_000,
            service_ids=[1, 2],
            service_count=2,
        )

    def test_predict_with_and_without_agreed_completion(self):
        without_budget = self.stub.PredictRepair(self.request, timeout=5)
        self.assertFalse(without_budget.HasField("delay_probability"))
        self.assertTrue(math.isfinite(without_budget.estimated_duration_minutes))
        self.assertGreater(without_budget.estimated_duration_minutes, 0)

        self.request.planned_budget_minutes = 120
        with_budget = self.stub.PredictRepair(self.request, timeout=5)
        self.assertTrue(with_budget.HasField("delay_probability"))
        self.assertTrue(0 <= with_budget.delay_probability <= 1)
        self.assertEqual(with_budget.estimated_duration_minutes, without_budget.estimated_duration_minutes)

    def test_invalid_budgets(self):
        for budget in (0, -1, float("nan"), float("inf")):
            with self.subTest(budget=budget):
                self.request.planned_budget_minutes = budget
                self.assert_invalid_request()

    def test_zero_mileage_is_valid_but_missing_mileage_is_not(self):
        self.request.mileage_km = 0
        self.stub.PredictRepair(self.request, timeout=5)
        self.request.ClearField("mileage_km")
        self.assert_invalid_request()

    def assert_invalid_request(self):
        with self.assertRaises(grpc.RpcError) as error:
            self.stub.PredictRepair(self.request, timeout=5)
        self.assertEqual(error.exception.code(), grpc.StatusCode.INVALID_ARGUMENT)


if __name__ == "__main__":
    unittest.main()
