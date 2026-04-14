import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { getPatients, getServiceCodes, getIndications, getAuthRuleForServiceCode, submitPriorAuthRequest } from "../api/PriorAuthApi";
import type { Patient } from "../types/Patient";
import type { ServiceCode } from "@/types/ServiceCode";
import type { Indication } from "@/types/Indication";
import type { FormField } from "@/types/AuthRule";
import type { AuthRequest } from "@/types/AuthRequest";
import { z } from "zod";
import { submitPARequestSchema, buildDynamicFieldSchema, type SubmitPARequestFormValues } from "@/schemas/submitPARequest";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export default function SubmitPARequest() {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [serviceCodes, setServiceCodes] = useState<ServiceCode[]>([]);
  const [indications, setIndications] = useState<Indication[]>([]);
  const [loadingForm, setLoadingForm] = useState(true);
  const [loadingIndications, setLoadingIndications] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [dynamicFields, setDynamicFields] = useState<FormField[]>([]);
  const [dynamicValues, setDynamicValues] = useState<Record<string, string | number | boolean>>({});
  const [dynamicErrors, setDynamicErrors] = useState<Record<string, string>>({});
  const [requestType, setRequestType] = useState<"Service" | "Medication" | null>(null);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<SubmitPARequestFormValues>({
    resolver: zodResolver<any, SubmitPARequestFormValues, any>(submitPARequestSchema),
  });

  const selectedServiceCode = watch("serviceCode");
  const selectedIndicationCode = watch("indicationCode");

  useEffect(() => {
    Promise.all([getPatients(), getServiceCodes()])
      .then(([p, s]) => {
        setPatients(p);
        setServiceCodes(s);
      })

      .catch(() => setFormError("Could not load form data."))
      .finally(() => setLoadingForm(false));
  }, []);

  useEffect(() => {
    if (!selectedServiceCode) {
      setIndications([]);
      setDynamicFields([]);
      setDynamicValues({});
      setRequestType(null);
      return;
    }
    setLoadingIndications(true);
    setValue("indicationCode", "");
    setDynamicFields([]);
    setDynamicValues({});
    setDynamicErrors({});
    getIndications(selectedServiceCode)
      .then((results) => {
        setIndications(results);
        if (results.length === 1) {
          setValue("indicationCode", results[0].indicationCode, { shouldValidate: true });
        }
      })
      .catch(() => setIndications([]))
      .finally(() => setLoadingIndications(false));
  }, [selectedServiceCode, setValue]);

  useEffect(() => {
    if (!selectedServiceCode || !selectedIndicationCode) {
      setDynamicFields([]);
      setDynamicValues({});
      setRequestType(null);
      return;
    }
    getAuthRuleForServiceCode(selectedServiceCode, selectedIndicationCode)
      .then((rule) => {
        const fields = rule.formDefinition.fields;
        setRequestType(rule.requestType);
        const allFields = rule.requestType === "Medication"
          ? [...fields, ...(rule.formDefinition.medicationFields ?? [])]
          : fields;
        setDynamicFields(allFields);
        setDynamicValues(Object.fromEntries(
          allFields.map((f) => [f.name, f.defaultValue ?? (f.type === "boolean" ? false : "")])
        ));
        setDynamicErrors({});
      })
      .catch(() => {
        setDynamicFields([]);
        setDynamicValues({});
        setDynamicErrors({});
        setRequestType(null);
      });
  }, [selectedServiceCode, selectedIndicationCode]);

  const onSubmit = async (data: SubmitPARequestFormValues) => {
    const dynamicValidation = buildDynamicFieldSchema(dynamicFields).safeParse(dynamicValues);
    if (!dynamicValidation.success) {
      const errors: Record<string, string> = {};
      for (const [key, issues] of Object.entries(z.flattenError(dynamicValidation.error).fieldErrors)) {
        errors[key] = issues?.[0] ?? "Invalid value";
      }
      setDynamicErrors(errors);
      return;
    }
    setDynamicErrors({});

    const request: AuthRequest = {
        patientId: data.patientId,
        practitionerId: 1, // Hardcoded for demo purposes
        priority: "routine", // Hardcoded for demo purposes
        code: {
            code: data.serviceCode,
            system: serviceCodes.find(s => s.code === data.serviceCode)?.codeSystem ?? "",
            display: serviceCodes.find(s => s.code === data.serviceCode)?.displayName || data.serviceCode,
        },
        reasonCode: [data.indicationCode],
        clinicalData: dynamicValues,
        medicationRequest: requestType === "Medication" ? {
            medication: {
                code: data.serviceCode,
                system: serviceCodes.find(s => s.code === data.serviceCode)?.codeSystem ?? "",
                display: serviceCodes.find(s => s.code === data.serviceCode)?.displayName || data.serviceCode,
            },
            intent: "order",
            dosageInstructionText: dynamicValues["dosageInstructionText"] as string,
            quantityValue: typeof dynamicValues["quantityValue"] === "number" ? dynamicValues["quantityValue"] as number : undefined,
            quantityUnit: dynamicValues["quantityUnit"] as string,
            numberOfRepeatsAllowed: typeof dynamicValues["numberOfRepeatsAllowed"] === "number" ? dynamicValues["numberOfRepeatsAllowed"] as number : undefined,
            expectedSupplyDurationDays: typeof dynamicValues["expectedSupplyDurationDays"] === "number" ? dynamicValues["expectedSupplyDurationDays"] as number : undefined,
        } : undefined,
    };

    console.log("Submit:", { request });
    try {
      await submitPriorAuthRequest(request);
      alert("Prior auth request submitted successfully!");
    } catch (error) {
      console.error("Submission error:", error);
      alert("Failed to submit prior auth request.");
    }
  };

  if (loadingForm) return <p className="p-6 text-sm text-muted-foreground">Loading...</p>;
  if (formError) return <p className="p-6 text-sm text-destructive">{formError}</p>;

  return (
    <div className="p-6 max-w-2xl mx-auto space-y-6">
      <h1 className="text-2xl font-semibold">Submit PA Request</h1>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">Request Details</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">

            {/* Patient */}
            <div className="space-y-1">
              <label className="text-sm font-medium" htmlFor="patientId">Patient</label>
              <select
                id="patientId"
                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                defaultValue=""
                {...register("patientId")}
              >
                <option value="" disabled>Select a patient…</option>
                {patients.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.fullName} — {p.age}yo, {p.gender}
                  </option>
                ))}
              </select>
              {errors.patientId && (
                <p className="text-xs text-destructive">{errors.patientId.message}</p>
              )}
            </div>

            {/* Service Code */}
            <div className="space-y-1">
              <label className="text-sm font-medium" htmlFor="serviceCode">Service</label>
              <select
                id="serviceCode"
                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                defaultValue=""
                {...register("serviceCode")}
              >
                <option value="" disabled>Select a service…</option>
                {serviceCodes.map((s) => (
                  <option key={s.code} value={s.code}>
                    {s.code} — {s.displayName}
                  </option>
                ))}
              </select>
              {errors.serviceCode && (
                <p className="text-xs text-destructive">{errors.serviceCode.message}</p>
              )}
            </div>

            {/* Indication */}
            <div className="space-y-1">
              <label className="text-sm font-medium" htmlFor="indicationCode">Indication</label>
              <select
                id="indicationCode"
                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm disabled:opacity-50"
                defaultValue=""
                disabled={!selectedServiceCode || loadingIndications}
                {...register("indicationCode")}
              >
                <option value="" disabled>
                  {loadingIndications ? "Loading…" : !selectedServiceCode ? "Select a service first…" : "Select an indication…"}
                </option>
                {indications.map((i) => (
                  <option key={i.indicationCode} value={i.indicationCode}>
                    {i.indicationDisplayName}
                  </option>
                ))}
              </select>
              {errors.indicationCode && (
                <p className="text-xs text-destructive">{errors.indicationCode.message}</p>
              )}
            </div>

            {/* Dynamic fields from formDefinition */}
            {dynamicFields.map((field) => (
              <div key={field.name} className="space-y-1">
                <label className="text-sm font-medium" htmlFor={field.name}>
                  {field.label}
                  {!field.validation.required && <span className="text-muted-foreground ml-1">(optional)</span>}
                </label>

                {field.type === "boolean" && (
                  <input
                    id={field.name}
                    type="checkbox"
                    className="ml-2"
                    checked={dynamicValues[field.name] as boolean}
                    onChange={(e) =>
                      setDynamicValues((prev) => ({ ...prev, [field.name]: e.target.checked }))
                    }
                  />
                )}

                {field.type === "select" && (
                  <select
                    id={field.name}
                    className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                    value={dynamicValues[field.name] as string}
                    onChange={(e) =>
                      setDynamicValues((prev) => ({ ...prev, [field.name]: e.target.value }))
                    }
                  >
                    <option value="" disabled>Select…</option>
                    {field.options?.map((opt: string) => (
                      <option key={opt} value={opt}>{opt}</option>
                    ))}
                  </select>
                )}

                {(field.type === "text" || field.type === "number") && (
                  <input
                    id={field.name}
                    type={field.type}
                    className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                    value={dynamicValues[field.name] as string}
                    onChange={(e) =>
                      setDynamicValues((prev) => ({
                        ...prev,
                        [field.name]: field.type === "number" ? e.target.valueAsNumber : e.target.value,
                      }))
                    }
                  />
                )}

                {dynamicErrors[field.name] && (
                  <p className="text-xs text-destructive">{dynamicErrors[field.name]}</p>
                )}
              </div>
            ))}

            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Submitting…" : "Submit Request"}
            </Button>

          </form>
        </CardContent>
      </Card>
    </div>
  );
}
