"""Tune duration models on validation days and evaluate on later orders."""

import argparse
import hashlib
import json
import platform
from pathlib import Path

import joblib
import numpy as np
import sklearn
from sklearn.base import clone
from sklearn.dummy import DummyRegressor
from sklearn.ensemble import RandomForestRegressor
from sklearn.feature_extraction import DictVectorizer
from sklearn.linear_model import Ridge
from sklearn.metrics import mean_absolute_error
from sklearn.model_selection import ParameterGrid
from sklearn.pipeline import make_pipeline
from sklearn.preprocessing import StandardScaler

from dataset import chronological_split, load_repairs


def xy(rows):
    return [row.features for row in rows], [row.duration for row in rows]


def fit_candidate(estimator, params, train, validation):
    fitted = clone(estimator).set_params(**params).fit(*xy(train))
    features, actual = xy(validation)
    return float(mean_absolute_error(actual, fitted.predict(features)))


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

    estimators = {
        "median": (DummyRegressor(strategy="median"), {}),
        "ridge": (make_pipeline(DictVectorizer(sparse=False), StandardScaler(), Ridge()),
                  {"ridge__alpha": [0.1, 1.0, 10.0, 100.0]}),
        "random_forest": (make_pipeline(DictVectorizer(sparse=False),
                                       RandomForestRegressor(n_estimators=200, random_state=42, n_jobs=1)),
                          {"randomforestregressor__max_depth": [6, 12, None],
                           "randomforestregressor__min_samples_leaf": [1, 3, 8]}),
    }

    results = {}
    for name, (estimator, grid) in estimators.items():
        trials = [
            {"params": params, "validation_mae_minutes": fit_candidate(estimator, params, train, validation)}
            for params in ParameterGrid(grid)
        ]
        best = min(trials, key=lambda trial: trial["validation_mae_minutes"])
        results[name] = {**best, "trials": trials}

    # Lock the winner before evaluating any configuration on the test data.
    selected = min(("ridge", "random_forest"), key=lambda name: results[name]["validation_mae_minutes"])
    test_features, test_actual = xy(test)
    args.output.mkdir(parents=True, exist_ok=True)
    for name, (estimator, _) in estimators.items():
        result = results[name]
        fitted = clone(estimator).set_params(**result["params"]).fit(*xy(train + validation))
        predictions = fitted.predict(test_features)
        result["test_mae_minutes"] = float(mean_absolute_error(test_actual, predictions))
        if name == selected:
            model_path = args.output / "duration.joblib"
            joblib.dump(fitted, model_path)
            np.testing.assert_allclose(joblib.load(model_path).predict(test_features[:3]), predictions[:3])
        print(f"{name}: validation MAE {result['validation_mae_minutes']:.2f}, "
              f"test MAE {result['test_mae_minutes']:.2f} minutes")

    report = {
        "dataset_sha256": hashlib.sha256(args.input.read_bytes()).hexdigest(),
        "python": platform.python_version(),
        "scikit_learn": sklearn.__version__,
        "numpy": np.__version__,
        "splits": splits,
        "selection_metric": "validation_mae_minutes",
        "selected_model": selected,
        "models": results,
    }
    (args.output / "metrics.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"Selected: {selected}. Results: {args.output}")


if __name__ == "__main__":
    main()
