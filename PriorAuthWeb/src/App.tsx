import { Routes, Route, Navigate } from "react-router-dom";
import PrescriberDashboard from "./pages/PrescriberDashboard";
import SubmitPARequest from "./pages/SubmitPARequest";
import ReviewerDashboard from "./pages/ReviewerDashboard";
import ReviewerDetail from "./pages/ReviewerDetail";
import './App.css'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="/dashboard" element={<PrescriberDashboard />} />
      <Route path="/submit" element={<SubmitPARequest />} />
      <Route path="/reviewer" element={<ReviewerDashboard />} />
      <Route path="/reviewer/:id" element={<ReviewerDetail />} />
    </Routes>
  );
}
