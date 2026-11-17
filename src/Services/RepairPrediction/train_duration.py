"""Train the selected Ridge model and evaluate it on later orders."""

import argparse
import hashlib
import json
import platform
from pathlib import Path

import joblib
import numpy as np
import sklearn
from sklearn.feature_extraction import DictVectorizer
from sklearn.linear_model import Ridge
from sklearn.metrics import mean_absolute_error
from sklearn.pipeline import make_pipeline
from sklearn.preprocessing import StandardScaler

from dataset import chronological_split, load_repairs


RIDGE_ALPHA = 0.1  # Selected in notebooks/repair_duration.ipynb.


def xy(rows):
    return [row.features for row in rows], [row.duration for row in rows]


def describe(rows):
    return {
        "orders": len(rows),
        "from": rows[0].appointment.date().isoformat(),
        "to": rows[-1].appointment.date().isoformat(),
    }


def main():
    repo = Path(__file__).resolve().parents[3]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, default=repo / "artifacts/ml/repair-orders.csv")
    parser.add_argument("--output", type=Path, default=repo / "artifacts/ml/duration")
    args = parser.parse_args()

    train, validation, test = chronological_split(load_repairs(args.input))
    splits = {name: describe(rows) for name, rows in
              (("train", train), ("validation", validation), ("test", test))}
    for name, info in splits.items():
        print(f"{name}: {info['orders']} orders ({info['from']} to {info['to']})")

    fitted = make_pipeline(DictVectorizer(sparse=False), StandardScaler(), Ridge(alpha=RIDGE_ALPHA))
    fitted.fit(*xy(train + validation))
    test_features, test_actual = xy(test)
    predictions = fitted.predict(test_features)
    test_mae = float(mean_absolute_error(test_actual, predictions))

    args.output.mkdir(parents=True, exist_ok=True)
    model_path = args.output / "duration.joblib"
    joblib.dump(fitted, model_path)
    np.testing.assert_allclose(joblib.load(model_path).predict(test_features), predictions)

    report = {
        "dataset_sha256": hashlib.sha256(args.input.read_bytes()).hexdigest(),
        "python": platform.python_version(),
        "scikit_learn": sklearn.__version__,
        "numpy": np.__version__,
        "splits": splits,
        "selected_model": "ridge",
        "models": {"ridge": {"params": {"ridge__alpha": RIDGE_ALPHA}, "test_mae_minutes": test_mae}},
    }
    (args.output / "metrics.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"Ridge (alpha={RIDGE_ALPHA:g}): test MAE {test_mae:.2f} minutes. Results: {args.output}")


if __name__ == "__main__":
    main()
