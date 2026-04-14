export interface AuthRequest {
    patientId: number;
    practitionerId: number;
    priority: "routine" | "urgent" | "stat";
    code: {
        code: string;
        system: string;
        display: string;
    };
    reasonCode: string[];
    clinicalData?: Record<string, any>;
    medicationRequest?: {
        medication: {
            code: string;
            system: string;
            display: string;
        };
        intent: string;
        dosageInstructionText?: string;
        quantityValue?: number;
        quantityUnit?: string;
        numberOfRepeatsAllowed?: number;
        expectedSupplyDurationDays?: number;
    };
}