import api from "./api";

export const createOrder = (data: any) =>
  api.post("/Orders/Order", data);

export const createOrderItem = (data: any) =>
  api.post("/Orders/OrderItem", data);