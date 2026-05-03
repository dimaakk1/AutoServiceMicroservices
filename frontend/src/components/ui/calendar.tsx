import * as React from "react";
import { DayPicker } from "react-day-picker";
import { ChevronLeft, ChevronRight } from "lucide-react";

import { cn } from "../../lib/utils";
import { buttonVariants } from "../../components/ui/button";

export type CalendarProps = React.ComponentProps<typeof DayPicker>;

export function Calendar({
  className,
  classNames,
  ...props
}: CalendarProps) {
  return (
    <DayPicker
      mode="single"
      weekStartsOn={1}
      className={cn("p-3 w-full", className)}
      classNames={{
        months: "w-full flex flex-col",
        month: "w-full space-y-4",

        caption:
          "flex justify-center relative items-center mb-4 font-medium",

        nav: "flex items-center gap-2",
        nav_button: cn(
          buttonVariants({ variant: "outline", size: "icon" }),
          "h-8 w-8"
        ),
        nav_button_previous: "absolute left-1",
        nav_button_next: "absolute right-1",

        table: "w-full border-collapse",

        /* 🔥 ГОЛОВНЕ ВИПРАВЛЕННЯ */
        head_row: "grid grid-cols-7 w-full",
        head_cell:
          "text-muted-foreground text-xs text-center py-2 font-medium",

        row: "grid grid-cols-7 w-full",
        cell: "flex items-center justify-center aspect-square",

        day: cn(
          buttonVariants({ variant: "ghost" }),
          "h-9 w-9 p-0 font-normal"
        ),

        day_selected:
          "bg-accent text-accent-foreground hover:bg-accent/90",

        day_today: "bg-primary text-primary-foreground",

        day_outside: "text-muted-foreground opacity-40",

        day_disabled: "opacity-40",

        day_hidden: "invisible",

        ...classNames,
      }}

      components={{
        Chevron: ({ orientation }) =>
          orientation === "left" ? (
            <ChevronLeft className="h-4 w-4" />
          ) : (
            <ChevronRight className="h-4 w-4" />
          ),
      }}
      {...props}
    />
  );
}