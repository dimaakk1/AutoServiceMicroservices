import axios from "axios";
import api from "./api";

export interface Vehicle {
  id: string;
  vin: string;
  make: string;
  model: string;
  year: number;
  mileageKm: number | null;
  licensePlate: string | null;
  color: string | null;
  createdAt: string;
  updatedAt: string;
}

export type SaveVehicle = Omit<Vehicle, "id" | "createdAt" | "updatedAt">;

export const getVehicles = (signal?: AbortSignal) => api.get<Vehicle[]>("/vehicles", { signal });
export const getVehicle = (id: string, signal?: AbortSignal) =>
  api.get<Vehicle>(`/vehicles/${encodeURIComponent(id)}`, { signal });
export const createVehicle = (data: SaveVehicle) => api.post<Vehicle>("/vehicles", data);
export const updateVehicle = (id: string, data: SaveVehicle) =>
  api.put<Vehicle>(`/vehicles/${encodeURIComponent(id)}`, data);
export const deleteVehicle = (id: string) => api.delete(`/vehicles/${encodeURIComponent(id)}`);

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
