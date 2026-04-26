import api from "./api";

// profile
export const getProfile = () => api.get("/users/me");

export const updateProfile = (data: any) =>
  api.put("/users/me", data);

// password
export const changePassword = (data: any) =>
  api.post("/users/change-password", data);