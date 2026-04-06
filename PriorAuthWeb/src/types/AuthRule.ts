export type FormFieldType = "boolean" | "select" | "number" | "text";

export interface FormField {
    name: string;
    label: string;
    type: FormFieldType;
    options?: string[];
    required?: boolean;
}

export interface FormDefinition {
    fields: FormField[];
}

export interface AuthRule {
    id: number;
    requestType: "Service" | "Medication";
    codeSystem: string; // e.g. "HCPCS"
    code: string; // e.g. "J0135"
    indicationCode: string; // e.g. "M06.9"
    displayName: string; // e.g. "Humira (adalimumab)"
    formDefinition: FormDefinition;
    ruleDefinition: Record<string, string | number | boolean>;
    isActive: boolean;
    effectiveDate: string; // ISO date string
    createdAt: string; // ISO date string
}