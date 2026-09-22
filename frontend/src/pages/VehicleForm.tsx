import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { ArrowLeft, CheckCircle2, Info, Loader2, Save, Search } from "lucide-react";
import { toast } from "sonner";
import { createVehicle, decodeVin, getVehicle, updateVehicle, vehicleError, vinDecodeError } from "../api/vehicle";
import type { SaveVehicle } from "../api/vehicle";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import VehicleLoadState from "../components/VehicleLoadState";

const emptyForm = { vin: "", make: "", model: "", year: "", mileageKm: "", licensePlate: "", color: "" };
type Field = keyof typeof emptyForm;
type FieldErrors = Partial<Record<Field, string>>;
const fields: { name: Field; label: string; placeholder: string; maxLength?: number; required?: boolean; numeric?: boolean }[] = [
  { name: "make", label: "Марка", placeholder: "Volkswagen", maxLength: 100, required: true },
  { name: "model", label: "Модель", placeholder: "Golf", maxLength: 100, required: true },
  { name: "year", label: "Рік випуску", placeholder: "2010", required: true, numeric: true },
  { name: "mileageKm", label: "Пробіг, км", placeholder: "150000", numeric: true },
  { name: "licensePlate", label: "Номерний знак", placeholder: "AA1234BB", maxLength: 20 },
  { name: "color", label: "Колір", placeholder: "Сірий", maxLength: 50 },
];

export default function VehicleForm() {
  const { id } = useParams();
  return <Editor key={id ?? "new"} id={id} />;
}

function Editor({ id }: { id?: string }) {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [form, setForm] = useState(() => ({
    ...emptyForm,
    vin: id ? "" : (searchParams.get("vin") ?? "").trim().toUpperCase().slice(0, 17),
  }));
  const [loading, setLoading] = useState(Boolean(id));
  const [loadError, setLoadError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [errors, setErrors] = useState<FieldErrors>({});
  const [decoding, setDecoding] = useState(false);
  const [decodeError, setDecodeError] = useState<string | null>(null);
  const [decoded, setDecoded] = useState(false);
  const backTo = id ? `/my-vehicles/${id}` : "/my-vehicles";

  useEffect(() => {
    if (!id) return;
    const controller = new AbortController();
    getVehicle(id, controller.signal)
      .then(({ data }) => {
        if (!controller.signal.aborted) setForm({
          vin: data.vin, make: data.make, model: data.model, year: String(data.year),
          mileageKm: data.mileageKm === null ? "" : String(data.mileageKm),
          licensePlate: data.licensePlate ?? "", color: data.color ?? "",
        });
      })
      .catch((err: unknown) => { if (!controller.signal.aborted) setLoadError(vehicleError(err)); })
      .finally(() => { if (!controller.signal.aborted) setLoading(false); });
    return () => controller.abort();
  }, [id, attempt]);

  const change = (field: Field, value: string) => {
    setForm((current) => ({ ...current, [field]: value }));
    setErrors((current) => ({ ...current, [field]: undefined }));
    setError(null);
    if (field === "vin") {
      setDecodeError(null);
      setDecoded(false);
    }
  };

  const decode = async () => {
    const vin = form.vin.trim().toUpperCase();
    if (!/^[A-HJ-NPR-Z0-9]{17}$/.test(vin)) {
      setErrors((current) => ({ ...current, vin: "Введіть 17 латинських літер або цифр, без I, O та Q." }));
      document.getElementById("vehicle-vin")?.focus();
      return;
    }
    setDecoding(true);
    setDecodeError(null);
    setDecoded(false);
    setErrors((current) => ({ ...current, vin: undefined }));
    try {
      const { data } = await decodeVin(vin);
      setForm((current) => ({
        ...current,
        vin: data.vin,
        make: data.make ?? current.make,
        model: data.model ?? current.model,
        year: data.modelYear === null ? current.year : String(data.modelYear),
      }));
      setDecoded(true);
    } catch (err) {
      setDecodeError(vinDecodeError(err));
    } finally {
      setDecoding(false);
    }
  };

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (saving || decoding) return;
    const nextErrors: FieldErrors = {};
    const vin = form.vin.trim().toUpperCase();
    const year = Number(form.year);
    const mileageKm = form.mileageKm.trim() === "" ? null : Number(form.mileageKm);
    if (!/^[A-HJ-NPR-Z0-9]{17}$/.test(vin)) nextErrors.vin = "Введіть 17 латинських літер або цифр, без I, O та Q.";
    if (!form.make.trim()) nextErrors.make = "Вкажіть марку автомобіля.";
    if (!form.model.trim()) nextErrors.model = "Вкажіть модель автомобіля.";
    if (!Number.isInteger(year) || year < 1886 || year > new Date().getUTCFullYear() + 1) nextErrors.year = "Вкажіть коректний рік — не пізніше наступного календарного року.";
    if (mileageKm !== null && (!Number.isInteger(mileageKm) || mileageKm < 0 || mileageKm > 2147483647)) nextErrors.mileageKm = "Пробіг має бути цілим невід’ємним числом до 2 147 483 647 км.";
    setErrors(nextErrors);
    setError(null);
    const firstInvalid = Object.keys(nextErrors)[0];
    if (firstInvalid) {
      document.getElementById(`vehicle-${firstInvalid}`)?.focus();
      return;
    }
    const data: SaveVehicle = {
      vin, make: form.make.trim(), model: form.model.trim(), year, mileageKm,
      licensePlate: form.licensePlate.trim().toUpperCase() || null, color: form.color.trim() || null,
    };
    setSaving(true);
    try {
      const response = id ? await updateVehicle(id, data) : await createVehicle(data);
      toast.success(id ? "Дані автомобіля оновлено" : "Автомобіль додано");
      navigate(`/my-vehicles/${response.data.id}`, { replace: true });
    } catch (err) {
      setError(vehicleError(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="min-h-[70vh] bg-muted/30">
      <div className="container max-w-4xl py-8 sm:py-12">
        <Link to={backTo} className="mb-6 inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground"><ArrowLeft className="h-4 w-4" /> {id ? "До автомобіля" : "Мої автомобілі"}</Link>
        <h1 className="mb-3 text-3xl font-bold tracking-tight sm:text-4xl">{id ? "Редагування автомобіля" : "Новий автомобіль"}</h1>
        <p className="mb-8 text-muted-foreground">{id ? "Оновіть характеристики або поточний пробіг вашого авто." : "Заповніть основні дані, щоб зберегти авто у своєму гаражі."}</p>

        {loading || loadError ? <VehicleLoadState loading={loading} error={loadError} onRetry={() => { setLoading(true); setLoadError(null); setAttempt((n) => n + 1); }} /> : (
          <form noValidate onSubmit={submit} className="rounded-2xl border bg-card shadow-sm" aria-busy={saving}>
            <fieldset disabled={saving} className="space-y-6 p-6 sm:p-8">
              <legend className="sr-only">Дані автомобіля</legend>
              <p className="text-sm text-muted-foreground">Поля із * обов’язкові.</p>
              <div className="space-y-2">
                <Label htmlFor="vehicle-vin">VIN-код *</Label>
                <div className="flex flex-col gap-3 sm:flex-row">
                  <Input id="vehicle-vin" name="vin" value={form.vin} onChange={(e) => change("vin", e.target.value.trim().toUpperCase())}
                    placeholder="WVWZZZ1KZAW000001" maxLength={17} required autoCapitalize="characters" autoComplete="off" spellCheck={false}
                    className="font-mono tracking-wider" aria-invalid={Boolean(errors.vin)} aria-describedby="vehicle-vin-help vehicle-vin-error" />
                  <Button type="button" variant="outline" className="shrink-0" disabled={decoding} onClick={decode}>
                    {decoding ? <Loader2 className="animate-spin" /> : <Search />}{decoding ? "Перевіряємо…" : "Заповнити за VIN"}
                  </Button>
                </div>
                <p id="vehicle-vin-help" className="text-xs text-muted-foreground">17 символів. VIN можна знайти у свідоцтві про реєстрацію авто.</p>
                <p id="vehicle-vin-error" className="text-sm text-destructive" aria-live="polite">{errors.vin}</p>
                {decodeError && <p role="alert" className="rounded-lg bg-destructive/5 p-3 text-sm text-destructive">{decodeError}</p>}
                {decoded && <p className="flex items-center gap-2 rounded-lg bg-emerald-50 p-3 text-sm text-emerald-700"><CheckCircle2 className="h-4 w-4 shrink-0" /> Отримані дані заповнено. Перевірте їх перед збереженням.</p>}
              </div>
              <div className="grid gap-5 sm:grid-cols-2">
                {fields.map((field) => (
                  <div key={field.name} className="space-y-2">
                    <Label htmlFor={`vehicle-${field.name}`}>{field.label}{field.required ? " *" : ""}</Label>
                    <Input id={`vehicle-${field.name}`} name={field.name} type={field.numeric ? "number" : "text"}
                      inputMode={field.numeric ? "numeric" : undefined} step={field.numeric ? 1 : undefined}
                      min={field.name === "year" ? 1886 : field.numeric ? 0 : undefined}
                      max={field.name === "year" ? new Date().getUTCFullYear() + 1 : field.numeric ? 2147483647 : undefined}
                      maxLength={field.maxLength} required={field.required} placeholder={field.placeholder}
                      value={form[field.name]} onChange={(e) => change(field.name, e.target.value)}
                      aria-invalid={Boolean(errors[field.name])} aria-describedby={errors[field.name] ? `vehicle-${field.name}-error` : undefined} />
                    {errors[field.name] && <p id={`vehicle-${field.name}-error`} className="text-sm text-destructive" role="alert">{errors[field.name]}</p>}
                  </div>
                ))}
              </div>
              <div className="flex items-start gap-3 rounded-xl bg-muted/50 p-4 text-sm text-muted-foreground"><Info className="mt-0.5 h-4 w-4 shrink-0" /><p>Дані заповнюються з бази NHTSA і можуть бути неповними для деяких авто. Звірте результат із документами — усі поля можна виправити вручну.</p></div>
            </fieldset>
            {error && <p role="alert" className="mx-6 mb-6 rounded-xl border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive sm:mx-8">{error}</p>}
            <div className="flex flex-wrap gap-3 border-t p-6 sm:px-8">
              <Button type="submit" variant="accent" disabled={saving || decoding}>{saving ? <Loader2 className="animate-spin" /> : <Save />}{saving ? "Збереження…" : "Зберегти автомобіль"}</Button>
              <Button type="button" variant="outline" disabled={saving} onClick={() => navigate(backTo)}>Скасувати</Button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
