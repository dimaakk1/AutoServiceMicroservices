import api from "./api";

export const getServices = () => {
  return api.get("/Catalog/Service");
};