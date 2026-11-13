import api from "./api";

export const getTakenSlots = (date: string) =>
  api.get("/Orders/Order/taken-slots", {
    params: { date },
  });

export const getOrdersWithItems = () => {
  return api.get("/Orders/OrderItem/with-items");
};

export const createOrder = (data: {
  orderDate: string;
  status: string;
  vehicleId: string | null;
}) => {
  return api.post("/Orders/Order", {
    orderId: 0,
    orderDate: data.orderDate,
    status: data.status,
    vehicleId: data.vehicleId,
  });
};

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

export const updateOrder = (data: {
  orderId: number;
  orderDate: string;
  status: string;
}) => {
  return api.put("/Orders/Order", {
    orderId: data.orderId,
    orderDate: data.orderDate,
    status: data.status,
  });
};

export const getMechanicOrders = () =>
  api.get("/Orders/OrderItem/mechanic-with-items");

export const getOrderItems = (orderId: number) =>
  api.get(`/Orders/OrderItem/order/${orderId}`);

export const assignMechanic = (
  orderId: number,
  mechanic: { mechanicId: string; mechanicName: string }
) => api.put(`/Orders/Order/${orderId}/assign-mechanic`, mechanic);

export const updateMechanicOrderStatus = (orderId: number, status: string) =>
  api.put(`/Orders/Order/${orderId}/mechanic-status`, { status });

export const updateAdminOrderStatus = (orderId: number, status: string) =>
  api.put(`/Orders/Order/${orderId}/status`, { status });

export const setPlannedCompletion = (orderId: number, plannedCompletionAt: string) =>
  api.put(`/Orders/Order/${orderId}/planned-completion`, { plannedCompletionAt });
