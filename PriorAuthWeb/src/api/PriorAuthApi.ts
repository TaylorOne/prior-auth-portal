import { type IPublicClientApplication, InteractionRequiredAuthError } from "@azure/msal-browser";
import type { PriorAuthSummary } from "@/types/PriorAuth";
import type { Patient } from "@/types/Patient";
import type { Practitioner } from "@/types/Practitioner";
import type { ServiceCode } from "@/types/ServiceCode";
import type { Indication } from "@/types/Indication";
import type { AuthRule } from "@/types/AuthRule";
import type { AuthRequest } from "@/types/AuthRequest";
import type { ReviewQueueItem, PriorAuthDetail } from "@/types/ReviewQueue";
import { apiRequest } from "@/auth/msalConfig";

const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5054";

let _msalInstance: IPublicClientApplication | null = null;

export function initAuth(instance: IPublicClientApplication) {
  _msalInstance = instance;
}

async function getAccessToken(): Promise<string> {
  if (!_msalInstance) throw new Error("MSAL not initialized");
  const account = _msalInstance.getActiveAccount() ?? _msalInstance.getAllAccounts()[0];
  if (!account) throw new Error("No authenticated account");
  _msalInstance.setActiveAccount(account);
  try {
    const result = await _msalInstance.acquireTokenSilent({ ...apiRequest, account });
    return result.accessToken;
  } catch (e) {
    if (e instanceof InteractionRequiredAuthError) {
      await _msalInstance.acquireTokenRedirect(apiRequest);
      return "";
    }
    throw e;
  }
}

async function authHeaders(): Promise<HeadersInit> {
  const token = await getAccessToken();
  return { Authorization: `Bearer ${token}` };
}

export async function getPriorAuthRequests(): Promise<PriorAuthSummary[]> {
  const response = await fetch(`${BASE_URL}/priorauth`, { headers: await authHeaders() });
  if (!response.ok) throw new Error("Failed to fetch prior auth requests");
  return response.json();
}

export async function getPatients(): Promise<Patient[]> {
  const response = await fetch(`${BASE_URL}/patients`, { headers: await authHeaders() });
  if (!response.ok) throw new Error("Failed to fetch patients");
  return response.json();
}

export async function getCurrentPractitioner(): Promise<Practitioner> {
  const response = await fetch(`${BASE_URL}/practitioners/me`, { headers: await authHeaders() });
  if (!response.ok) throw new Error("Failed to fetch current practitioner");
  return response.json();
}

export async function getServiceCodes(): Promise<ServiceCode[]> {
  const response = await fetch(`${BASE_URL}/authrules/codes`, { headers: await authHeaders() });
  if (!response.ok) throw new Error("Failed to fetch service codes");
  return response.json();
}

export async function getIndications(serviceCode: string): Promise<Indication[]> {
  const response = await fetch(`${BASE_URL}/authrules/${encodeURIComponent(serviceCode)}/indications`, { headers: await authHeaders() });
  if (!response.ok) throw new Error("Failed to fetch indications");
  return response.json();
}

export async function getAuthRuleForServiceCode(serviceCode: string, indicationCode: string): Promise<AuthRule> {
  const response = await fetch(`${BASE_URL}/authrules/${encodeURIComponent(serviceCode)}/${encodeURIComponent(indicationCode)}`, { headers: await authHeaders() });
  if (!response.ok) throw new Error("Failed to fetch auth rule for service code");
  return response.json();
}

export async function getReviewQueue(): Promise<ReviewQueueItem[]> {
  const response = await fetch(`${BASE_URL}/priorauth/review-queue`, { headers: await authHeaders() });
  if (!response.ok) throw new Error("Failed to fetch review queue");
  return response.json();
}

export async function getPriorAuthDetail(id: number): Promise<PriorAuthDetail> {
  const response = await fetch(`${BASE_URL}/priorauth/${id}`, { headers: await authHeaders() });
  if (!response.ok) throw new Error("Failed to fetch prior auth detail");
  return response.json();
}

export async function submitReviewDecision(
  id: number,
  decision: string,
  reviewerNotes: string | null
): Promise<void> {
  const response = await fetch(`${BASE_URL}/priorauth/${id}/decision`, {
    method: "PATCH",
    headers: { ...await authHeaders(), "Content-Type": "application/json" },
    body: JSON.stringify({ decision, reviewerNotes }),
  });
  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new Error(body?.error ?? "Failed to submit decision");
  }
}

export async function submitPriorAuthRequest(priorAuthRequest: AuthRequest): Promise<AuthRequest> {
  const response = await fetch(`${BASE_URL}/priorauth`, {
    method: "POST",
    headers: { ...await authHeaders(), "Content-Type": "application/json" },
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
