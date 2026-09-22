import axios from "axios";

const apiBaseUrl = import.meta.env.VITE_API_URL ?? "http://localhost:5000/api";

const api = axios.create({
  baseURL: apiBaseUrl,
});

const isAuthenticationRequest = (url: string | undefined) => {
  if (!url) return false;

  const pathname = url.split("?")[0].replace(/^https?:\/\/[^/]+/i, "");
  return /\/auth\/(login|register|refresh)\/?$/.test(pathname);
};

const redirectToLogin = () => {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");

  if (window.location.pathname !== "/auth") {
    window.location.href = "/auth";
  }
};

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});


api.interceptors.response.use(
  (res) => res,
  async (err) => {
    const original = err.config;

    // A wrong login/password is also a 401. Let the form show that error;
    // only protected requests are allowed to refresh a session.
    if (
      err.response?.status === 401 &&
      original &&
      !original._retry &&
      !isAuthenticationRequest(original.url)
    ) {
      original._retry = true;

      try {
        const refreshToken = localStorage.getItem("refreshToken");

        if (!refreshToken) {
          redirectToLogin();
          return Promise.reject(err);
        }

        const res = await axios.post(
          `${apiBaseUrl}/auth/refresh`,
          { refreshToken }
        );

        const { accessToken, refreshToken: newRefresh } = res.data;

        localStorage.setItem("accessToken", accessToken);
        localStorage.setItem("refreshToken", newRefresh);

        original.headers.Authorization = `Bearer ${accessToken}`;

        return api(original);
      } catch {
        redirectToLogin();
      }
    }

    return Promise.reject(err);
  }
);

export default api;
