import type { PriorAuthSummary } from "../types/PriorAuth";
import type { Patient } from "../types/Patient";
import type { ServiceCode } from "@/types/ServiceCode";

const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5054";

export async function getPriorAuthRequests(): Promise<PriorAuthSummary[]> {
  const response = await fetch(`${BASE_URL}/priorauth`);
  if (!response.ok) throw new Error("Failed to fetch prior auth requests");
  return response.json();
}

export async function getPatients(): Promise<Patient[]> {
  const response = await fetch(`${BASE_URL}/patients`);
  if (!response.ok) throw new Error("Failed to fetch patients");
  return response.json();
}

export async function getServiceCodes(): Promise<ServiceCode[]> {
  const response = await fetch(`${BASE_URL}/authrules/`);
  if (!response.ok) throw new Error("Failed to fetch service codes");
  return response.json();
}

