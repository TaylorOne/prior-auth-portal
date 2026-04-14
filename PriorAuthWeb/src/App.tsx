import { Routes, Route, Navigate } from "react-router-dom";
import PrescriberDashboard from "./pages/PrescriberDashboard";
import SubmitPARequest from "./pages/SubmitPARequest";
import './App.css'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="/dashboard" element={<PrescriberDashboard />} />
      <Route path="/submit" element={<SubmitPARequest />} />
    </Routes>
  );
}
