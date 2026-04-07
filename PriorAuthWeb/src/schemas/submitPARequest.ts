import { z } from "zod";
import type { FormField } from "@/types/AuthRule";

export const submitPARequestSchema = z.object({
    patientId: z.coerce.number().positive("Select a patient"),
    serviceCode: z.string().min(1, "Select a service"),
    indicationCode: z.string().min(1, "Select an indication"),
});

export type SubmitPARequestFormValues = z.infer<typeof submitPARequestSchema>;

export function buildDynamicFieldSchema(fields: FormField[]) {
    const shape: Record<string, z.ZodTypeAny> = {};
    for (const field of fields) {
        const { required, maxLength, min, max, integer } = field.validation;
        if (field.type === "boolean") {
            shape[field.name] = z.boolean();
        } else if (field.type === "number") {
            let schema: z.ZodNumber = z.number({ invalid_type_error: `${field.label} must be a number` });
            if (min !== undefined) schema = schema.min(min, `${field.label} must be at least ${min}`);
            if (max !== undefined) schema = schema.max(max, `${field.label} must be at most ${max}`);
            if (integer) schema = schema.int(`${field.label} must be a whole number`);
            shape[field.name] = required ? schema : schema.optional();
        } else {
            let schema: z.ZodString = z.string();
            if (required) schema = schema.min(1, `${field.label} is required`);
            if (maxLength !== undefined) schema = schema.max(maxLength, `${field.label} must be ${maxLength} characters or fewer`);
            shape[field.name] = required ? schema : schema.optional();
        }
    }
    return z.object(shape);
}
