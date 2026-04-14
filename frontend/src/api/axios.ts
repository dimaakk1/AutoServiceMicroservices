import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:5000/api",
});

// =====================
// attach token
// =====================
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

// =====================
// refresh logic
// =====================
api.interceptors.response.use(
  (res) => res,
  async (err) => {
    const original = err.config;

    if (err.response?.status === 401 && !original._retry) {
      original._retry = true;

      try {
        const refreshToken = localStorage.getItem("refreshToken");

        if (!refreshToken) throw new Error("No refresh token");

        const res = await axios.post(
          "https://localhost:5000/api/auth/refresh",
          { refreshToken }
        );

        const { accessToken, refreshToken: newRefresh } = res.data;

        localStorage.setItem("accessToken", accessToken);
        localStorage.setItem("refreshToken", newRefresh);

        original.headers.Authorization = `Bearer ${accessToken}`;

        return api(original);
      } catch (e) {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");

        window.location.href = "/auth";
      }
    }

    return Promise.reject(err);
  }
);

export default api;