import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Toaster } from "sonner";
import { AuthProvider } from "./lib/auth-context";
import Index from "./pages/Index";
import Services from "./pages/Services";
import Auth from "./pages/Auth";
import Layout from "./components/Layout";

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
      <Layout>
        <Toaster richColors position="top-right" />

          <Routes>
            <Route path="/" element={<Index />} />
            <Route path="/services" element={<Services />} />
            <Route path="/auth" element={<Auth />} />
          </Routes>
      </Layout>

      </BrowserRouter>

    </AuthProvider>
    
  );
}