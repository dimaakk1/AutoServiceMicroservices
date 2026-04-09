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

export const services: Service[] = [
  { id: "1", name: "Заміна масла", description: "Повна заміна моторної оливи та фільтра з використанням преміум матеріалів", price: "від 800 ₴", duration: "30 хв", icon: Droplets, category: "Технічне обслуговування" },
  { id: "2", name: "Діагностика двигуна", description: "Комп'ютерна діагностика всіх систем автомобіля", price: "від 500 ₴", duration: "45 хв", icon: Gauge, category: "Діагностика" },
  { id: "3", name: "Ремонт гальм", description: "Заміна гальмівних колодок, дисків та перевірка системи", price: "від 1200 ₴", duration: "1-2 год", icon: Shield, category: "Ремонт" },
  { id: "4", name: "Шиномонтаж", description: "Сезонна заміна шин, балансування та перевірка тиску", price: "від 400 ₴", duration: "40 хв", icon: CircleDot, category: "Шини" },
  { id: "5", name: "Ремонт підвіски", description: "Діагностика та ремонт елементів підвіски автомобіля", price: "від 2000 ₴", duration: "2-4 год", icon: Car, category: "Ремонт" },
  { id: "6", name: "Заміна акумулятора", description: "Підбір та встановлення нового акумулятора", price: "від 600 ₴", duration: "20 хв", icon: Zap, category: "Електрика" },
  { id: "7", name: "Заміна ременя ГРМ", description: "Заміна ременя газорозподільного механізму та ролика", price: "від 3500 ₴", duration: "3-5 год", icon: Settings, category: "Ремонт" },
  { id: "8", name: "Кузовний ремонт", description: "Фарбування, рихтування, усунення подряпин та вм'ятин", price: "від 1500 ₴", duration: "1-3 дні", icon: Wrench, category: "Кузов" },
];

export const initialReviews: Review[] = [
  { id: "1", author: "Олександр М.", rating: 5, text: "Чудовий сервіс! Замінили масло дуже швидко і якісно. Рекомендую всім.", date: "2026-03-10", serviceUsed: "Заміна масла" },
  { id: "2", author: "Ірина К.", rating: 4, text: "Професійна діагностика, знайшли проблему, яку інші не бачили. Ціна адекватна.", date: "2026-03-08", serviceUsed: "Діагностика двигуна" },
  { id: "3", author: "Василь П.", rating: 5, text: "Зробили ремонт підвіски за день. Машина їде як нова! Дуже задоволений.", date: "2026-03-05", serviceUsed: "Ремонт підвіски" },
  { id: "4", author: "Наталія С.", rating: 5, text: "Швидкий шиномонтаж, привітний персонал. Завжди сюди їжджу.", date: "2026-02-28", serviceUsed: "Шиномонтаж" },
];

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
