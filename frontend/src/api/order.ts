import api from "./api";

// створити замовлення
export const createOrder = (data: {
  orderDate: string;
  status: string;
}) => {
  return api.post("/Orders/Order", {
    orderId: 0,
    orderDate: data.orderDate,
    status: data.status,
  });
};

// додати послугу в замовлення
export const addOrderItem = (data: {
  orderId: number;
  productId: number;
  quantity: number;
}) => {
  return api.post("/Orders/OrderItem", {
    orderItemId: 0,
    orderId: data.orderId,
    productId: data.productId,
    quantity: data.quantity,
  });
};