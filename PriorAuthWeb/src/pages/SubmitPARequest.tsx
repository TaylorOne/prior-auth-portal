import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { getPatients, getServiceCodes, getIndications } from "../api/PriorAuthApi";
import type { Patient } from "../types/Patient";
import type { ServiceCode } from "@/types/ServiceCode";
import type { Indication } from "@/types/Indication";
import { submitPARequestSchema, type SubmitPARequestFormValues } from "@/schemas/submitPARequest";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export default function SubmitPARequest() {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [serviceCodes, setServiceCodes] = useState<ServiceCode[]>([]);
  const [indications, setIndications] = useState<Indication[]>([]);
  const [loadingForm, setLoadingForm] = useState(true);
  const [loadingIndications, setLoadingIndications] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<SubmitPARequestFormValues>({
    resolver: zodResolver(submitPARequestSchema),
  });

  const selectedServiceCode = watch("serviceCode");

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
      return;
    }
    setLoadingIndications(true);
    setValue("indicationCode", "");
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

  const onSubmit = async (data: SubmitPARequestFormValues) => {
    console.log("Submit:", data);
    // TODO: wire up POST endpoint
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

            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Submitting…" : "Submit Request"}
            </Button>

          </form>
        </CardContent>
      </Card>
    </div>
  );
}
