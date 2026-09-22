import api from "./api";

export const getReviews = () => {
  return api.get("/Reviews");
};

export const createReview = (data: {
  orderId: number;
  rating: number;
  comment: string;
}) => {
  return api.post("/Reviews", {
    orderId: data.orderId,
    rating: data.rating,
    comment: data.comment,
  });
};