import type { PriorAuthSummary } from "@/types/PriorAuth";
import type { Patient } from "@/types/Patient";
import type { ServiceCode } from "@/types/ServiceCode";
import type { Indication } from "@/types/Indication";
import type { AuthRule } from "@/types/AuthRule";
import type { AuthRequest } from "@/types/AuthRequest";

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
  const response = await fetch(`${BASE_URL}/authrules/codes`);
  if (!response.ok) throw new Error("Failed to fetch service codes");
  return response.json();
}

export async function getIndications(serviceCode: string): Promise<Indication[]> {
  const response = await fetch(`${BASE_URL}/authrules/${encodeURIComponent(serviceCode)}/indications`);
  if (!response.ok) throw new Error("Failed to fetch indications");
  return response.json();
}

export async function getAuthRuleForServiceCode(serviceCode: string, indicationCode: string): Promise<AuthRule> {
  const response = await fetch(`${BASE_URL}/authrules/${encodeURIComponent(serviceCode)}/${encodeURIComponent(indicationCode)}`);
  if (!response.ok) throw new Error("Failed to fetch auth rule for service code");
  return response.json();
}

export async function submitPriorAuthRequest(priorAuthRequest: AuthRequest): Promise<AuthRequest> {
  const response = await fetch(`${BASE_URL}/priorauth`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(priorAuthRequest),
  });
  if (!response.ok) {
    const body = await response.json().catch(() => null);
    const message =
      body?.errors?.join(", ") ??
      body?.error ??
      (typeof body === "string" ? body : "Failed to submit prior auth request");
    throw new Error(message);
  }
  return response.json();
}