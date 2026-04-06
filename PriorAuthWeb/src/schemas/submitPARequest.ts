import { z } from "zod";

export const submitPARequestSchema = z.object({
    patientId: z.coerce.number().positive("Select a patient"),
    serviceCode: z.string().min(1, "Select a service"),
    indicationCode: z.string().min(1, "Select an indication"),
});

export type SubmitPARequestFormValues = z.infer<typeof submitPARequestSchema>;
