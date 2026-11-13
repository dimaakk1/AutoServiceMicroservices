"""Read the order export and prepare features available before repair."""

import csv
import json
import math
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path


@dataclass(frozen=True)
class Repair:
    order_id: int
    appointment: datetime
    features: dict[str, str | int]
    duration: float


def make_features(appointment, mechanic_id, vehicle_year, mileage_km, service_ids, service_count):
    features = {
        "mechanic": mechanic_id,
        "vehicle_age_years": appointment.year - vehicle_year,
        "mileage_km": mileage_km,
        "weekday": str(appointment.weekday()),
        "appointment_hour": str(appointment.hour),
        "service_count": service_count,
    }
    features.update({f"service_{service_id}": 1 for service_id in service_ids})
    return features


def load_repairs(path: Path) -> list[Repair]:
    required = {
        "order_id", "appointment_local", "mechanic_id", "vehicle_year", "mileage_km",
        "service_ids", "service_count", "duration_minutes",
    }
    repairs = []
    seen = set()
    with path.open(encoding="utf-8-sig", newline="") as source:
        reader = csv.DictReader(source)
        missing = required - set(reader.fieldnames or [])
        if missing:
            raise ValueError(f"Missing CSV columns: {', '.join(sorted(missing))}")
        for line, row in enumerate(reader, start=2):
            try:
                order_id = int(row["order_id"])
                appointment = datetime.fromisoformat(row["appointment_local"])
                year = int(row["vehicle_year"])
                mileage = int(row["mileage_km"])
                count = int(row["service_count"])
                services = json.loads(row["service_ids"])
                mechanic = row["mechanic_id"].strip()
                duration = float(row["duration_minutes"])
                if (
                    order_id <= 0 or order_id in seen or appointment.tzinfo is not None
                    or not 1886 <= year <= appointment.year or mileage < 0 or not mechanic
                    or not math.isfinite(duration) or duration <= 0
                    or not isinstance(services, list) or not services
                    or any(type(service) is not int or service <= 0 for service in services)
                    or len(set(services)) != len(services) or count < len(services)
                ):
                    raise ValueError("Duplicate ID or invalid repair values")
            except (ValueError, TypeError, AttributeError) as error:
                raise ValueError(f"Invalid CSV row {line}: {error}") from error
            seen.add(order_id)
            repairs.append(Repair(order_id, appointment, make_features(
                appointment, mechanic, year, mileage, services, count,
            ), duration))
    return sorted(repairs, key=lambda repair: (repair.appointment, repair.order_id))


def chronological_split(repairs: list[Repair]):
    """Choose day boundaries closest to 70% and 85% of the ordered rows."""
    boundaries = [
        i for i in range(1, len(repairs))
        if repairs[i - 1].appointment.date() != repairs[i].appointment.date()
    ]
    if len(boundaries) < 2:
        raise ValueError("At least three distinct appointment days are required")
    train_end = min(boundaries[:-1], key=lambda i: abs(i - len(repairs) * 0.70))
    validation_end = min(
        (i for i in boundaries if i > train_end), key=lambda i: abs(i - len(repairs) * 0.85),
    )
    return repairs[:train_end], repairs[train_end:validation_end], repairs[validation_end:]
