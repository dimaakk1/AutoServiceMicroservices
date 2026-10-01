import { useState } from "react";
import type { FormEvent, ReactNode } from "react";
import { Link } from "react-router-dom";
import { AlertTriangle, Car, CheckCircle2, Loader2, Search, Wrench } from "lucide-react";
import { decodeVin, vinDecodeError } from "../api/vehicle";
import type { VinDecode } from "../api/vehicle";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { useAuth } from "../lib/auth-context";
import { PageHeader } from "../components/PageHeader";

const vinPattern = /^[A-HJ-NPR-Z0-9]{17}$/;

function Value({ label, children }: { label: string; children: ReactNode }) {
  if (children === null || children === undefined || children === "") return null;
  return (
    <div className="border border-border bg-background/70 p-4">
      <dt className="text-xs font-medium uppercase tracking-wider text-muted-foreground">{label}</dt>
      <dd className="mt-1 break-words font-medium">{children}</dd>
    </div>
  );
}

export default function VinDecoder() {
  const { user } = useAuth();
  const [vin, setVin] = useState("");
  const [result, setResult] = useState<VinDecode | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const normalized = vin.trim().toUpperCase();
    setVin(normalized);
    setResult(null);
    if (!vinPattern.test(normalized)) {
      setError("VIN має містити 17 латинських літер або цифр, без I, O та Q.");
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const { data } = await decodeVin(normalized);
      setResult(data);
    } catch (err) {
      setError(vinDecodeError(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-[75vh] bg-muted/20">
      <PageHeader
        eyebrow="Безкоштовна перевірка"
        title="VIN-декодер автомобіля"
        description="Дізнайтеся марку, модель, рік, двигун, тип кузова та інші заводські характеристики автомобіля."
      >
        <div className="grid h-14 w-14 place-items-center border border-accent/40 bg-accent/10 text-accent"><Car className="h-7 w-7" /></div>
      </PageHeader>

      <div className="container max-w-5xl py-10 md:py-14">
        <form onSubmit={submit} noValidate className="border border-border bg-card p-6 shadow-card sm:p-8">
          <Label htmlFor="vin-decoder" className="text-base">VIN-код</Label>
          <div className="mt-3 flex flex-col gap-3 sm:flex-row">
            <Input id="vin-decoder" value={vin} maxLength={17} autoComplete="off" spellCheck={false} autoCapitalize="characters"
              onChange={(event) => { setVin(event.target.value.trim().toUpperCase()); setError(null); setResult(null); }}
              placeholder="WVWZZZ1KZAW000001" className="h-12 flex-1 font-mono text-base tracking-wider" aria-invalid={Boolean(error)} />
            <Button type="submit" variant="accent" className="h-12 px-6" disabled={loading}>
              {loading ? <Loader2 className="animate-spin" /> : <Search />}{loading ? "Перевіряємо…" : "Розшифрувати VIN"}
            </Button>
          </div>
          <p className="mt-3 text-sm text-muted-foreground">VIN містить 17 символів і вказаний у свідоцтві про реєстрацію та на кузові авто.</p>
          {error && <p role="alert" className="mt-4 border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">{error}</p>}
        </form>

        {result && (
          <section className="mt-8 space-y-6" aria-live="polite">
            <div className="flex flex-col gap-5 border border-border bg-card p-6 shadow-card sm:flex-row sm:items-center sm:justify-between sm:p-8">
              <div>
                <p className="mb-2 flex items-center gap-2 text-sm font-medium text-emerald-600"><CheckCircle2 className="h-4 w-4" /> VIN розшифровано</p>
                <h2 className="text-3xl font-bold">{[result.make, result.model].filter(Boolean).join(" ") || result.manufacturer || "Автомобіль"}</h2>
                <p className="mt-2 font-mono text-sm tracking-wide text-muted-foreground">{result.vin}</p>
              </div>
              <Button asChild variant="accent" className="shrink-0">
                <Link to={user ? `/my-vehicles/new?vin=${encodeURIComponent(result.vin)}` : "/auth"}>
                  <Car /> {user ? "Додати до моїх авто" : "Увійти, щоб додати"}
                </Link>
              </Button>
            </div>

            {result.warnings.length > 0 && (
              <div className="flex gap-3 border border-amber-500/30 bg-amber-500/10 p-5 text-amber-300">
                <AlertTriangle className="mt-0.5 h-5 w-5 shrink-0" />
                <div>{result.warnings.map((warning) => <p key={warning}>{warning}</p>)}</div>
              </div>
            )}

            <div className="border border-border bg-card p-6 shadow-card sm:p-8">
              <h3 className="mb-5 flex items-center gap-2 text-xl font-semibold"><Car className="text-accent" /> Основні характеристики</h3>
              <dl className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                <Value label="Рік випуску">{result.modelYear}</Value><Value label="Виробник">{result.manufacturer}</Value>
                <Value label="Тип авто">{result.vehicleType}</Value><Value label="Тип кузова">{result.bodyClass}</Value>
                <Value label="Кількість дверей">{result.doors}</Value><Value label="Серія / комплектація">{[result.series, result.trim].filter(Boolean).join(" · ")}</Value>
              </dl>
            </div>

            <div className="border border-border bg-card p-6 shadow-card sm:p-8">
              <h3 className="mb-5 flex items-center gap-2 text-xl font-semibold"><Wrench className="text-accent" /> Двигун і трансмісія</h3>
              <dl className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                <Value label="Тип пального">{result.fuelType}</Value><Value label="Об’єм двигуна">{result.engineDisplacementLiters ? `${result.engineDisplacementLiters} л` : null}</Value>
                <Value label="Циліндри">{result.engineCylinders}</Value><Value label="Модель двигуна">{result.engineModel}</Value>
                <Value label="Трансмісія">{[result.transmission, result.transmissionSpeeds ? `${result.transmissionSpeeds} передач` : null].filter(Boolean).join(" · ")}</Value>
                <Value label="Привід">{result.driveType}</Value>
              </dl>
            </div>

            {(result.plantCountry || result.plantCity || result.plantCompany) && (
              <div className="border border-border bg-card p-6 shadow-card sm:p-8">
                <h3 className="mb-5 text-xl font-semibold">Виробництво</h3>
                <dl className="grid gap-3 sm:grid-cols-3"><Value label="Країна">{result.plantCountry}</Value><Value label="Місто">{result.plantCity}</Value><Value label="Завод">{result.plantCompany}</Value></dl>
              </div>
            )}
          </section>
        )}

        <div className="mt-8 border border-border bg-muted/60 p-5 text-sm text-muted-foreground">
          <strong className="text-foreground">Важливо:</strong> декодер показує заводські характеристики з бази NHTSA. Він не показує власників, ДТП, страхові випадки, реальний пробіг або сервісну історію автомобіля.
        </div>
      </div>
    </div>
  );
}
