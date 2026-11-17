"""Train the selected logistic regression and evaluate it on later orders."""

import argparse
import hashlib
import json
import platform
from pathlib import Path

import joblib
import numpy as np
import sklearn
from sklearn.feature_extraction import DictVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import brier_score_loss, confusion_matrix, precision_score, recall_score, roc_auc_score
from sklearn.pipeline import make_pipeline
from sklearn.preprocessing import StandardScaler

from dataset import chronological_split, load_repairs


THRESHOLD = 0.5
LOGISTIC_C = 10.0  # Selected in notebooks/delay_risk.ipynb.
LOGISTIC_MAX_ITER = 1000


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
        "brier_score": float(brier_score_loss(actual, scores, pos_label=1)),
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
    train_x, train_y = xy(train + validation)
    if set(train_y) != {0, 1}:
        parser.error("The combined training and validation data must include both on-time and delayed orders")

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

    fitted = make_pipeline(DictVectorizer(sparse=False), StandardScaler(),
                           LogisticRegression(C=LOGISTIC_C, max_iter=LOGISTIC_MAX_ITER))
    fitted.fit(train_x, train_y)
    test_x, test_y = xy(test)
    scores = probabilities(fitted, test_x)
    test_metrics = evaluate(test_y, scores)

    args.output.mkdir(parents=True, exist_ok=True)
    model_path = args.output / "delay.joblib"
    joblib.dump(fitted, model_path)
    np.testing.assert_allclose(probabilities(joblib.load(model_path), test_x), scores)

    report = {
        "dataset_sha256": hashlib.sha256(args.input.read_bytes()).hexdigest(),
        "python": platform.python_version(),
        "scikit_learn": sklearn.__version__,
        "numpy": np.__version__,
        "splits": splits,
        "selected_model": "logistic_regression",
        "classification_threshold": THRESHOLD,
        "models": {"logistic_regression": {
            "params": {"logisticregression__C": LOGISTIC_C, "logisticregression__max_iter": LOGISTIC_MAX_ITER},
            "test": test_metrics,
        }},
    }
    (args.output / "metrics.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"Logistic regression (C={LOGISTIC_C:g}): test Brier {test_metrics['brier_score']:.4f}. Results: {args.output}")


if __name__ == "__main__":
    main()
