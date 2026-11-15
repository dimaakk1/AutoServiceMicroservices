"""Tune delay classifiers on validation days and evaluate on later orders."""

import argparse
import hashlib
import json
import platform
from pathlib import Path

import joblib
import numpy as np
import sklearn
from sklearn.base import clone
from sklearn.dummy import DummyClassifier
from sklearn.ensemble import RandomForestClassifier
from sklearn.feature_extraction import DictVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import brier_score_loss, confusion_matrix, precision_score, recall_score, roc_auc_score
from sklearn.model_selection import ParameterGrid
from sklearn.pipeline import make_pipeline
from sklearn.preprocessing import StandardScaler

from dataset import chronological_split, load_repairs


THRESHOLD = 0.5


def xy(rows):
    return (
        [{**row.features, "planned_budget_minutes": row.planned_budget_minutes} for row in rows],
        [row.delayed for row in rows],
    )


def probabilities(fitted, features):
    positive_index = list(fitted.classes_).index(1)
    return fitted.predict_proba(features)[:, positive_index]


def evaluate(actual, scores):
    predicted = scores >= THRESHOLD
    tn, fp, fn, tp = confusion_matrix(actual, predicted, labels=[0, 1]).ravel()
    return {
        "brier_score": float(brier_score_loss(actual, scores)),
        "roc_auc": float(roc_auc_score(actual, scores)) if len(set(actual)) == 2 else None,
        "precision": float(precision_score(actual, predicted, zero_division=0)),
        "recall": float(recall_score(actual, predicted, zero_division=0)),
        "confusion_matrix": {"tn": int(tn), "fp": int(fp), "fn": int(fn), "tp": int(tp)},
    }


def main():
    repo = Path(__file__).resolve().parents[3]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, default=repo / "artifacts/ml/repair-orders.csv")
    parser.add_argument("--output", type=Path, default=repo / "artifacts/ml/delay")
    args = parser.parse_args()

    train, validation, test = chronological_split(load_repairs(args.input, include_delay=True))
    train_x, train_y = xy(train)
    validation_x, validation_y = xy(validation)
    if set(train_y) != {0, 1}:
        parser.error("The training split must include both on-time and delayed orders")

    splits = {}
    for name, rows in (("train", train), ("validation", validation), ("test", test)):
        delayed = sum(row.delayed for row in rows)
        splits[name] = {
            "orders": len(rows), "delayed": delayed,
            "from": rows[0].appointment.date().isoformat(),
            "to": rows[-1].appointment.date().isoformat(),
        }
        print(f"{name}: {len(rows)} orders, {delayed} delayed "
              f"({splits[name]['from']} to {splits[name]['to']})")

    estimators = {
        "prior": (make_pipeline(DictVectorizer(sparse=False), DummyClassifier(strategy="prior")), {}),
        "logistic_regression": (
            make_pipeline(DictVectorizer(sparse=False), StandardScaler(), LogisticRegression(max_iter=1000)),
            {"logisticregression__C": [0.01, 0.1, 1.0, 10.0]},
        ),
        "random_forest": (
            make_pipeline(DictVectorizer(sparse=False),
                          RandomForestClassifier(n_estimators=200, random_state=42, n_jobs=1)),
            {"randomforestclassifier__max_depth": [6, 12, None],
             "randomforestclassifier__min_samples_leaf": [1, 3, 8]},
        ),
    }

    results = {}
    for name, (estimator, grid) in estimators.items():
        trials = []
        for params in ParameterGrid(grid):
            fitted = clone(estimator).set_params(**params).fit(train_x, train_y)
            score = brier_score_loss(validation_y, probabilities(fitted, validation_x))
            trials.append({"params": params, "validation_brier_score": float(score)})
        best = min(trials, key=lambda trial: trial["validation_brier_score"])
        results[name] = {**best, "trials": trials}
        print(f"{name}: best validation Brier {best['validation_brier_score']:.4f}, {best['params']}")

    # Compare the baseline separately; lock model selection before scoring the holdout.
    selected = min(("logistic_regression", "random_forest"),
                   key=lambda name: results[name]["validation_brier_score"])
    test_x, test_y = xy(test)
    args.output.mkdir(parents=True, exist_ok=True)
    for name, (estimator, _) in estimators.items():
        result = results[name]
        fitted = clone(estimator).set_params(**result["params"]).fit(train_x + validation_x, train_y + validation_y)
        scores = probabilities(fitted, test_x)
        result["test"] = evaluate(test_y, scores)
        print(f"{name}: test Brier {result['test']['brier_score']:.4f}")
        if name == selected:
            model_path = args.output / "delay.joblib"
            joblib.dump(fitted, model_path)
            np.testing.assert_allclose(probabilities(joblib.load(model_path), test_x), scores)

    report = {
        "dataset_sha256": hashlib.sha256(args.input.read_bytes()).hexdigest(),
        "python": platform.python_version(),
        "scikit_learn": sklearn.__version__,
        "numpy": np.__version__,
        "splits": splits,
        "selection_metric": "validation_brier_score",
        "selected_model": selected,
        "beats_baseline_on_validation": (
            results[selected]["validation_brier_score"] < results["prior"]["validation_brier_score"]
        ),
        "classification_threshold": THRESHOLD,
        "models": results,
    }
    (args.output / "metrics.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"Selected: {selected}. Results: {args.output}")


if __name__ == "__main__":
    main()
