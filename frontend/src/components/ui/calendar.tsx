import * as React from "react";
import { DayPicker } from "react-day-picker";
import { cn } from "../../lib/utils";

export type CalendarProps = React.ComponentProps<typeof DayPicker>;

export function Calendar({
  className,
  classNames,
  ...props
}: CalendarProps) {
  return (
    <DayPicker
      className={cn(
        "p-4 bg-white rounded-xl border shadow-sm",
        className
      )}
      classNames={{
        root: "w-full",
        months: "w-full",
        month: "w-full",

        /* HEADER */
        month_caption:
          "flex items-center justify-center mb-3 gap-3",

        caption_label:
          "text-sm font-semibold text-orange-600",

        /* ❗ FIX: прибрали absolute */
        nav: "flex items-center gap-2",

        button_previous:
          "h-8 w-8 flex items-center justify-center rounded-lg border border-orange-200 text-orange-600 hover:bg-orange-50 transition",

        button_next:
          "h-8 w-8 flex items-center justify-center rounded-lg border border-orange-200 text-orange-600 hover:bg-orange-50 transition",

        /* GRID */
        month_grid: "w-full",

        weekdays:
          "flex justify-between text-xs text-muted-foreground mb-1",

        weekday: "w-10 text-center font-medium",

        week: "flex justify-between",

        day: "w-10 h-10 flex items-center justify-center",

        day_button:
          "w-10 h-10 rounded-lg flex items-center justify-center text-sm transition hover:bg-orange-100",

        selected:
          "bg-orange-500 text-white rounded-lg hover:bg-orange-600",

        today:
          "bg-orange-100 text-orange-700 font-semibold rounded-lg",

        outside: "opacity-40",
        disabled: "opacity-30 cursor-not-allowed",

        ...classNames,
      }}
      {...props}
    />
  );
}