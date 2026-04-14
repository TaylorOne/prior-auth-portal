export type FormFieldType = "boolean" | "select" | "number" | "text";

export interface FormField {
    name: string;
    label: string;
    type: FormFieldType;
    options?: string[];
    validation: ValidationRules;
    defaultValue?: string | number;
    editable?: boolean;
}

export interface ValidationRules {
    required: boolean;
    maxLength?: number;
    min?: number;
    max?: number;
    integer?: boolean;
}

export interface FormDefinition {
    fields: FormField[];
    medicationFields?: FormField[]; // Optional fields specific to medication requests
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