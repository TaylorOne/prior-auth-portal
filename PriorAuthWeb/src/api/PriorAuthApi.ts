import type { PriorAuthSummary } from "../types/PriorAuth";

const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5054";

export async function getPriorAuthRequests(): Promise<PriorAuthSummary[]> {
  const response = await fetch(`${BASE_URL}/priorauth`);
  if (!response.ok) throw new Error("Failed to fetch prior auth requests");
  return response.json();
}