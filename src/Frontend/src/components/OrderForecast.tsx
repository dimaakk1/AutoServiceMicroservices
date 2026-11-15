import { useEffect, useId, useRef, useState } from "react";
import type { ReactNode } from "react";
import { getDelayRisk, getEstimatedDuration } from "../api/order";
import { Button } from "./ui/button";

type ForecastState =
  | { status: "idle" | "loading" | "unavailable" | "error" }
  | { status: "ready"; value: number };

const formatPercent = new Intl.NumberFormat("uk-UA", { style: "percent", maximumFractionDigits: 1 });

function formatDuration(value: number) {
  const minutes = Math.round(value);
  if (minutes < 1) return "менше 1 хв";
  if (minutes < 60) return `${minutes} хв`;
  return `${Math.floor(minutes / 60)} год${minutes % 60 ? ` ${minutes % 60} хв` : ""}`;
}

export function OrderForecast({ orderId, kind, blockedBy = [] }: {
  orderId: number;
  kind: "duration" | "risk";
  blockedBy?: ReactNode[];
}) {
  const [state, setState] = useState<ForecastState>({ status: "idle" });
  const activeRequest = useRef<AbortController | null>(null);
  const isDuration = kind === "duration";
  const isBlocked = blockedBy.length > 0;
  const reasonsId = useId();

  useEffect(() => () => activeRequest.current?.abort(), []);

  const loadForecast = async () => {
    if (isBlocked || activeRequest.current) return;
    const request = new AbortController();
    activeRequest.current = request;
    setState({ status: "loading" });

    try {
      const getForecast = isDuration ? getEstimatedDuration : getDelayRisk;
      const value = await getForecast(orderId, request.signal);
      if (!request.signal.aborted) {
        setState(value === null ? { status: "unavailable" } : { status: "ready", value });
      }
    } catch {
      if (!request.signal.aborted) setState({ status: "error" });
    } finally {
      activeRequest.current = null;
    }
  };

  return (
    <div className="space-y-1 text-sm">
      <Button type="button" size="sm" variant="outline" onClick={loadForecast}
        disabled={isBlocked || state.status === "loading"} aria-describedby={isBlocked ? reasonsId : undefined}>
        {isDuration ? "Оцінити тривалість" : "Перевірити ризик затримки"}
      </Button>
      {isBlocked ? (
        <ul id={reasonsId} className="list-disc space-y-1 pl-5 text-muted-foreground">
          {blockedBy.map((reason, index) => <li key={index}>{reason}</li>)}
        </ul>
      ) : <div role="status">
        {state.status === "loading" && <p className="text-muted-foreground">Отримуємо прогноз…</p>}
        {state.status === "unavailable" && (
          <p className="text-muted-foreground">
            {isDuration
              ? "Прогноз наразі недоступний."
              : "Прогноз недоступний. Можливо, не вказано пробіг авто або сервіс прогнозування тимчасово недоступний."}
          </p>
        )}
        {state.status === "error" && <p className="text-muted-foreground">Не вдалося отримати прогноз. Спробуйте ще раз.</p>}
        {state.status === "ready" && (
          <>
            <p className="font-medium">
              {isDuration
                ? `Орієнтовна тривалість: ${formatDuration(state.value)}`
                : `Ймовірність затримки: ${formatPercent.format(state.value)}`}
            </p>
            <p className="text-xs text-muted-foreground">
              {isDuration ? "Це оцінка, а не час видачі авто." : "Відносно погодженого терміну завершення."}
            </p>
          </>
        )}
      </div>}
    </div>
  );
}
