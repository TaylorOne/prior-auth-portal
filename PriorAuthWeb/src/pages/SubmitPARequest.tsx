import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "react-router-dom";
import { AlertTriangle, Info, Loader2, Send } from "lucide-react";
import { z } from "zod";
import {
  getAuthRuleForServiceCode,
  getIndications,
  getPatients,
  getPractitioners,
  getServiceCodes,
  submitPriorAuthRequest,
} from "../api/PriorAuthApi";
import type { AuthRequest } from "@/types/AuthRequest";
import type { FormField } from "@/types/AuthRule";
import type { Indication } from "@/types/Indication";
import type { Patient } from "../types/Patient";
import type { Practitioner } from "../types/Practitioner";
import type { ServiceCode } from "@/types/ServiceCode";
import {
  buildDynamicFieldSchema,
  submitPARequestSchema,
  type SubmitPARequestFormValues,
} from "@/schemas/submitPARequest";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select } from "@/components/ui/select";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

function FieldLabel({
  htmlFor,
  label,
  description,
  optional,
}: {
  htmlFor: string;
  label: string;
  description?: string;
  optional?: boolean;
}) {
  return (
    <div className="flex min-h-5 items-center gap-2">
      <Label htmlFor={htmlFor}>{label}</Label>
      {optional && (
        <Badge variant="secondary" className="h-5 rounded-md px-1.5 text-[11px]">
          Optional
        </Badge>
      )}
      {description && (
        <Tooltip>
          <TooltipTrigger asChild>
            <button
              type="button"
              className="inline-flex size-5 items-center justify-center rounded-full text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              aria-label={`${label} help`}
            >
              <Info className="size-3.5" />
            </button>
          </TooltipTrigger>
          <TooltipContent>{description}</TooltipContent>
        </Tooltip>
      )}
    </div>
  );
}

export default function SubmitPARequest() {
  const navigate = useNavigate();
  const [practitioners, setPractitioners] = useState<Practitioner[]>([]);
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
  const [requiresManualReview, setRequiresManualReview] = useState(false);

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
    Promise.all([getPractitioners(), getPatients(), getServiceCodes()])
      .then(([pr, p, s]) => {
        setPractitioners(pr);
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
      setRequiresManualReview(false);
      return;
    }

    setLoadingIndications(true);
    setValue("indicationCode", "");
    setDynamicFields([]);
    setDynamicValues({});
    setDynamicErrors({});
    setRequiresManualReview(false);

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
      setRequiresManualReview(false);
      return;
    }

    getAuthRuleForServiceCode(selectedServiceCode, selectedIndicationCode)
      .then((rule) => {
        const fields = rule.formDefinition.fields;
        const allFields = rule.requestType === "Medication"
          ? [...fields, ...(rule.formDefinition.medicationFields ?? [])]
          : fields;

        setRequestType(rule.requestType);
        setRequiresManualReview(rule.requiresManualReview);
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
        setRequiresManualReview(false);
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

    const selectedService = serviceCodes.find((s) => s.code === data.serviceCode);
    const request: AuthRequest = {
      patientId: data.patientId,
      practitionerId: data.practitionerId,
      priority: "routine",
      code: {
        code: data.serviceCode,
        system: selectedService?.codeSystem ?? "",
        display: selectedService?.displayName || data.serviceCode,
      },
      reasonCode: [data.indicationCode],
      clinicalData: dynamicValidation.data,
      medicationRequest: requestType === "Medication" ? {
        medication: {
          code: data.serviceCode,
          system: selectedService?.codeSystem ?? "",
          display: selectedService?.displayName || data.serviceCode,
        },
        intent: "order",
        dosageInstructionText: dynamicValues["dosageInstructionText"] as string,
        quantityValue: typeof dynamicValues["quantityValue"] === "number"
          ? dynamicValues["quantityValue"] as number
          : undefined,
        quantityUnit: dynamicValues["quantityUnit"] as string,
        numberOfRepeatsAllowed: typeof dynamicValues["numberOfRepeatsAllowed"] === "number"
          ? dynamicValues["numberOfRepeatsAllowed"] as number
          : undefined,
        expectedSupplyDurationDays: typeof dynamicValues["expectedSupplyDurationDays"] === "number"
          ? dynamicValues["expectedSupplyDurationDays"] as number
          : undefined,
      } : undefined,
    };

    try {
      await submitPriorAuthRequest(request);
      navigate("/dashboard", {
        state: { priorAuthSubmitted: true },
        replace: true,
      });
    } catch (error) {
      console.error("Submission error:", error);
      alert("Failed to submit prior auth request.");
    }
  };

  if (loadingForm) {
    return (
      <div className="flex min-h-svh items-center justify-center bg-background p-6 text-sm text-muted-foreground">
        <Loader2 className="mr-2 size-4 animate-spin" />
        Loading form data...
      </div>
    );
  }

  if (formError) {
    return (
      <div className="min-h-svh bg-background p-6">
        <div className="mx-auto flex max-w-3xl items-center gap-2 rounded-md border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm text-destructive">
          <AlertTriangle className="size-4" />
          {formError}
        </div>
      </div>
    );
  }

  return (
    <TooltipProvider>
      <div className="min-h-svh bg-background px-4 py-6 text-foreground sm:px-6">
        <div className="mx-auto max-w-3xl space-y-6">
          <div className="space-y-2">
            <div className="flex flex-wrap items-center gap-2">
              <h1 className="m-0 text-2xl font-semibold tracking-normal">Submit PA Request</h1>
              {requestType && (
                <Badge variant="outline" className="rounded-md">
                  {requestType}
                </Badge>
              )}
            </div>
            <p className="text-sm text-muted-foreground">
              Capture the patient, service, indication, and payer criteria for this authorization.
            </p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <Card>
            <CardHeader>
              <CardTitle>Request Details</CardTitle>
              <CardDescription>Core information used to match the authorization rule.</CardDescription>
            </CardHeader>
            <CardContent className="grid gap-5 sm:grid-cols-2">
              <div className="space-y-2">
                <FieldLabel htmlFor="practitionerId" label="Practitioner" />
                <Select
                  id="practitionerId"
                  defaultValue=""
                  aria-invalid={!!errors.practitionerId}
                  {...register("practitionerId")}
                >
                  <option value="" disabled>Select a practitioner...</option>
                  {practitioners.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.fullName} - {p.specialty}
                    </option>
                  ))}
                </Select>
                {errors.practitionerId && (
                  <p className="text-xs text-destructive">{errors.practitionerId.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <FieldLabel htmlFor="patientId" label="Patient" />
                <Select
                  id="patientId"
                  defaultValue=""
                  aria-invalid={!!errors.patientId}
                  {...register("patientId")}
                >
                  <option value="" disabled>Select a patient...</option>
                  {patients.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.fullName} - {p.age}yo, {p.gender}
                    </option>
                  ))}
                </Select>
                {errors.patientId && (
                  <p className="text-xs text-destructive">{errors.patientId.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <FieldLabel htmlFor="serviceCode" label="Service or Medication" />
                <Select
                  id="serviceCode"
                  defaultValue=""
                  aria-invalid={!!errors.serviceCode}
                  {...register("serviceCode")}
                >
                  <option value="" disabled>Select a service...</option>
                  {serviceCodes.map((s) => (
                    <option key={s.code} value={s.code}>
                      {s.code} - {s.displayName}
                    </option>
                  ))}
                </Select>
                {errors.serviceCode && (
                  <p className="text-xs text-destructive">{errors.serviceCode.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <FieldLabel htmlFor="indicationCode" label="Indication" />
                <Select
                  id="indicationCode"
                  defaultValue=""
                  disabled={!selectedServiceCode || loadingIndications}
                  aria-invalid={!!errors.indicationCode}
                  {...register("indicationCode")}
                >
                  <option value="" disabled>
                    {loadingIndications
                      ? "Loading..."
                      : !selectedServiceCode
                        ? "Select a service first..."
                        : "Select an indication..."}
                  </option>
                  {indications.map((i) => (
                    <option key={i.indicationCode} value={i.indicationCode}>
                      {i.indicationDisplayName}
                    </option>
                  ))}
                </Select>
                {errors.indicationCode && (
                  <p className="text-xs text-destructive">{errors.indicationCode.message}</p>
                )}
              </div>

              {requiresManualReview && (
                <div className="flex items-start gap-2 rounded-md border bg-muted px-3 py-2 text-sm text-muted-foreground sm:col-span-2">
                  <AlertTriangle className="mt-0.5 size-4 shrink-0" />
                  This indication requires manual review by a clinical reviewer.
                </div>
              )}
            </CardContent>
          </Card>

          {dynamicFields.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Clinical Criteria</CardTitle>
                <CardDescription>Rule-specific documentation for this request.</CardDescription>
              </CardHeader>
              <CardContent className="grid gap-5">
                {dynamicFields.map((field) => (
                  <div key={field.name} className="space-y-2">
                    <FieldLabel
                      htmlFor={field.name}
                      label={field.label}
                      description={field.description}
                      optional={!field.validation.required}
                    />

                    {field.type === "boolean" && (
                      <div className="flex h-9 items-center gap-2 rounded-md border border-input bg-background px-3 shadow-xs">
                        <Checkbox
                          id={field.name}
                          checked={Boolean(dynamicValues[field.name])}
                          disabled={field.editable === false}
                          aria-invalid={!!dynamicErrors[field.name]}
                          onChange={(e) =>
                            setDynamicValues((prev) => ({ ...prev, [field.name]: e.target.checked }))
                          }
                        />
                        <Label htmlFor={field.name} className="font-normal text-muted-foreground">
                          Yes
                        </Label>
                      </div>
                    )}

                    {field.type === "select" && (
                      <Select
                        id={field.name}
                        value={(dynamicValues[field.name] as string) ?? ""}
                        disabled={field.editable === false}
                        aria-invalid={!!dynamicErrors[field.name]}
                        onChange={(e) =>
                          setDynamicValues((prev) => ({ ...prev, [field.name]: e.target.value }))
                        }
                      >
                        <option value="" disabled>Select...</option>
                        {field.options?.map((opt: string) => (
                          <option key={opt} value={opt}>{opt}</option>
                        ))}
                      </Select>
                    )}

                    {(field.type === "text" || field.type === "number") && (
                      <Input
                        id={field.name}
                        type={field.type}
                        value={(dynamicValues[field.name] as string | number) ?? ""}
                        disabled={field.editable === false}
                        aria-invalid={!!dynamicErrors[field.name]}
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
              </CardContent>
            </Card>
          )}

          <div className="flex justify-end">
            <Button
              type="submit"
              size="lg"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="size-4 animate-spin" />
                  Submitting...
                </>
              ) : (
                <>
                  <Send className="size-4" />
                  Submit Request
                </>
              )}
            </Button>
            </div>
          </form>
        </div>
      </div>
    </TooltipProvider>
  );
}
