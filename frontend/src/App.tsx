import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Toaster } from "sonner";

import { AuthProvider } from "./lib/auth-context";

import Layout from "./components/Layout";
import ProtectedRoute from "./components/ProtectedRoutes";

import Index from "./pages/Index";
import Services from "./pages/Services";
import Auth from "./pages/Auth";
import Booking from "./pages/Booking";
import Profile from "./pages/Profile";
import MyBookings from "./pages/MyBookings";
import Reviews from "./pages/Reviews";

import AdminDashboard from "./pages/admin/AdminDashboard";
import AdminOrders from "./pages/admin/AdminOrders";
import AdminServices from "./pages/admin/AdminServices";
import AdminReviews from "./pages/admin/AdminReviews";
import AdminUsers from "./pages/admin/AdminUsers";
import EmailConfirmed from "./pages/EmailConfirmed";


export default function App() {
  return (
      <AuthProvider>
      <BrowserRouter>
        <Layout>
          <Toaster
            richColors
            position="top-right"
          />

          <Routes>
            <Route path="/" element={<Index />} />

            <Route
              path="/services"
              element={<Services />}
            />

            <Route
              path="/auth"
              element={<Auth />}
            />

            <Route
              path="/reviews"
              element={<Reviews />}
            />
            <Route
              path="/email-confirmed"
              element={<EmailConfirmed />}
            />

            <Route
              path="/booking"
              element={
                <ProtectedRoute>
                  <Booking />
                </ProtectedRoute>
              }
            />

            <Route
              path="/profile"
              element={
                <ProtectedRoute>
                  <Profile />
                </ProtectedRoute>
              }
            />

            <Route
              path="/my-bookings"
              element={
                <ProtectedRoute>
                  <MyBookings />
                </ProtectedRoute>
              }
            />

            {/* ADMIN */}
            <Route
              path="/admin"
              element={
                <ProtectedRoute role="Admin">
                  <AdminDashboard />
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/orders"
              element={
                <ProtectedRoute role="Admin">
                  <AdminOrders />
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/services"
              element={
                <ProtectedRoute role="Admin">
                  <AdminServices />
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/reviews"
              element={
                <ProtectedRoute role="Admin">
                  <AdminReviews />
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/users"
              element={
                <ProtectedRoute role="Admin">
                  <AdminUsers />
                </ProtectedRoute>
              }
            />
          </Routes>
        </Layout>
      </BrowserRouter>
    </AuthProvider>    
  );
}