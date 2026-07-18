import api from "./api";

export const createCheckout = (data: {
  orderId: number;
  amount: number;
  description: string;
}) => {
  return api.post("/Orders/payments/checkout", data);
};

export const getPaymentByOrder = (orderId: number) => {
  return api.get(`/Orders/payments/order/${orderId}`);
};