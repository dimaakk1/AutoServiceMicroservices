import { Wrench, Car, Droplets, Settings, Shield, Gauge, Zap, CircleDot } from "lucide-react";

export interface Service {
  id: string;
  name: string;
  description: string;
  price: string;
  duration: string;
  icon: typeof Wrench;
  category: string;
}

export interface Booking {
  id: string;
  serviceId: string;
  serviceName: string;
  date: string;
  time: string;
  status: "pending" | "confirmed" | "in-progress" | "completed" | "cancelled";
  vehicleInfo: string;
}

export interface Review {
  id: string;
  author: string;
  rating: number;
  text: string;
  date: string;
  serviceUsed: string;
}




export const timeSlots = [
  "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
  "12:00", "12:30", "13:00", "13:30", "14:00", "14:30",
  "15:00", "15:30", "16:00", "16:30", "17:00", "17:30",
];

export const statusLabels: Record<Booking["status"], string> = {
  pending: "Очікує",
  confirmed: "Підтверджено",
  "in-progress": "В роботі",
  completed: "Завершено",
  cancelled: "Скасовано",
};

export const statusColors: Record<Booking["status"], string> = {
  pending: "bg-yellow-100 text-yellow-800",
  confirmed: "bg-blue-100 text-blue-800",
  "in-progress": "bg-accent/10 text-accent",
  completed: "bg-success/10 text-success",
  cancelled: "bg-destructive/10 text-destructive",
};
