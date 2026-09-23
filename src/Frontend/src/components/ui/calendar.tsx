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
        "border bg-card p-4 text-card-foreground shadow-card",
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
          "text-sm font-semibold text-accent",

        /* ❗ FIX: прибрали absolute */
        nav: "flex items-center gap-2",

        button_previous:
          "h-8 w-8 flex items-center justify-center border border-border text-accent hover:bg-muted transition",

        button_next:
          "h-8 w-8 flex items-center justify-center border border-border text-accent hover:bg-muted transition",

        /* GRID */
        month_grid: "w-full",

        weekdays:
          "flex justify-between text-xs text-muted-foreground mb-1",

        weekday: "w-10 text-center font-medium",

        week: "flex justify-between",

        day: "w-10 h-10 flex items-center justify-center",

        day_button:
          "w-10 h-10 flex items-center justify-center text-sm transition hover:bg-muted",

        selected:
          "bg-accent text-accent-foreground hover:bg-accent/90",

        today:
          "bg-accent/10 text-accent font-semibold",

        outside: "opacity-40",
        disabled: "opacity-30 cursor-not-allowed",

        ...classNames,
      }}
      {...props}
    />
  );
}
