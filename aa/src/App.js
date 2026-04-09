import { BrowserRouter, Routes, Route } from "react-router-dom";
import Index from "./pages/Index"; // твоя головна

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/index" element={<Index />} />

      </Routes>
    </BrowserRouter>
  );
}

export default App;
