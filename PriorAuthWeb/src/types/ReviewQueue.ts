export interface ReviewQueueItem {
  id: number;
  patientName: string;
  practitionerName: string;
  serviceCode: string;
  serviceCodeDisplay: string;
  createdAt: string;
}

export interface PriorAuthDetail {
  id: number;
  status: string;
  patientName: string;
  practitionerName: string;
  serviceCode: string;
  serviceCodeDisplay: string;
  createdAt: string;
  clinicalData: string;
}
