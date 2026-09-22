import axios from "axios";
import api from "./api";

export interface Vehicle {
  id: string;
  vin: string;
  make: string | null;
  model: string | null;
  year: number;
  mileageKm: number | null;
  licensePlate: string | null;
  color: string | null;
  createdAt: string;
  updatedAt: string;
}

export type SaveVehicle = Omit<Vehicle, "id" | "createdAt" | "updatedAt">;

export interface VinDecode {
  vin: string;
  make: string;
  model: string;
  modelYear: number | null;
  manufacturer: string | null;
  vehicleType: string | null;
  bodyClass: string | null;
  doors: number | null;
  fuelType: string | null;
  engineDisplacementLiters: number | null;
  engineCylinders: number | null;
  engineModel: string | null;
  transmission: string | null;
  transmissionSpeeds: number | null;
  driveType: string | null;
  series: string | null;
  trim: string | null;
  plantCountry: string | null;
  plantCity: string | null;
  plantCompany: string | null;
  isComplete: boolean;
  warnings: string[];
}

export const getVehicles = (signal?: AbortSignal) => api.get<Vehicle[]>("/vehicles", { signal });
export const getVehicle = (id: string, signal?: AbortSignal) =>
  api.get<Vehicle>(`/vehicles/${encodeURIComponent(id)}`, { signal });
export const createVehicle = (data: SaveVehicle) => api.post<Vehicle>("/vehicles", data);
export const updateVehicle = (id: string, data: SaveVehicle) =>
  api.put<Vehicle>(`/vehicles/${encodeURIComponent(id)}`, data);
export const deleteVehicle = (id: string) => api.delete(`/vehicles/${encodeURIComponent(id)}`);
export const decodeVin = (vin: string, signal?: AbortSignal) =>
  api.get<VinDecode>(`/vehicles/decode/${encodeURIComponent(vin.trim().toUpperCase())}`, { signal });

export function vehicleError(error: unknown): string {
  if (!axios.isAxiosError(error)) return "Не вдалося виконати дію. Спробуйте ще раз.";
  const status = error.response?.status;
  if (status === 404) return "Автомобіль не знайдено або він недоступний для вашого акаунта.";
  if (status === 401 || status === 403) return "Для доступу до автомобілів увійдіть у свій акаунт повторно.";
  if (status === 409) return "Авто з таким VIN уже є у вашому списку, або запис був змінений. Оновіть дані та спробуйте ще раз.";
  if (!status || status >= 500) return "Сервіс автомобілів зараз недоступний. Спробуйте пізніше.";
  const data = error.response?.data;
  if (data && typeof data === "object") {
    if (data.errors && typeof data.errors === "object") {
      const messages = Object.values(data.errors).flat().filter((value): value is string => typeof value === "string");
      if (messages.length) return messages.join(" ");
    }
    if (typeof data.title === "string") return data.title;
  }
  return "Перевірте введені дані та спробуйте ще раз.";
}

export function vinDecodeError(error: unknown): string {
  if (!axios.isAxiosError(error)) return "Не вдалося розшифрувати VIN. Спробуйте ще раз.";
  const status = error.response?.status;
  if (status === 404) return "NHTSA не знайшла достатньо даних за цим VIN. Перевірте код і спробуйте ще раз.";
  if (status === 503 || !status) return "Сервіс NHTSA тимчасово недоступний. Спробуйте трохи пізніше.";
  const data = error.response?.data;
  if (data && typeof data === "object") {
    if (data.errors && typeof data.errors === "object") {
      const messages = Object.values(data.errors).flat().filter((value): value is string => typeof value === "string");
      if (messages.length) return messages.join(" ");
    }
    if (typeof data.title === "string") return data.title;
  }
  return status === 400
    ? "VIN має містити 17 латинських літер або цифр, без I, O та Q."
    : "Не вдалося розшифрувати VIN. Спробуйте ще раз.";
}
