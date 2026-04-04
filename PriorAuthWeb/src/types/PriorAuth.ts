export interface PriorAuthSummary {
  id: number;
  status: string;
  priority: string;
  serviceCode: string;
  serviceCodeDisplay: string;
  patientName: string;
  practitionerName: string;
  specialty: string;
  createdAt: string;
  determinationDate: string | null;
}