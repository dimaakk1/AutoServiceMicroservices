import type { ReactNode } from "react";

type PageHeaderProps = {
  eyebrow?: string;
  title: ReactNode;
  description?: ReactNode;
  children?: ReactNode;
};

export function PageHeader({ eyebrow, title, description, children }: PageHeaderProps) {
  return (
    <section className="relative overflow-hidden border-b border-border bg-card">
      <div className="absolute inset-0 bg-grid-dark opacity-20" />
      <div className="container relative z-10 py-10 md:py-14">
        {eyebrow && (
          <div className="technical-label mb-4 flex items-center gap-3">
            <span className="h-px w-8 bg-accent" />
            {eyebrow}
          </div>
        )}
        <div className="flex flex-col gap-6 md:flex-row md:items-end md:justify-between">
          <div className="max-w-3xl">
            <h1 className="text-balance text-5xl leading-[0.9] md:text-7xl">{title}</h1>
            {description && (
              <p className="mt-4 max-w-2xl text-base leading-relaxed text-muted-foreground">{description}</p>
            )}
          </div>
          {children && <div className="shrink-0">{children}</div>}
        </div>
      </div>
    </section>
  );
}
