import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, CheckCircle2, Loader2, XCircle } from "lucide-react";
import { z } from "zod";
import { cn } from "@/lib/utils";
import { getPriorAuthDetail, submitReviewDecision } from "../api/PriorAuthApi";
import type { PriorAuthDetail } from "@/types/ReviewQueue";
import { getFieldLabel } from "@/lib/fieldLabels";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Label } from "@/components/ui/label";

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

function formatClinicalValue(value: unknown): string {
  if (typeof value === "boolean") return value ? "Yes" : "No";
  return String(value);
}

const decisionSchema = z
  .object({
    decision: z.enum(["Approved", "Denied"], {
      error: "Please select a decision.",
    }),
    reviewerNotes: z.string(),
  })
  .refine(
    (data) => data.decision !== "Denied" || data.reviewerNotes.trim().length > 0,
    { message: "A reason is required when denying", path: ["reviewerNotes"] }
  );

type DecisionFormValues = z.infer<typeof decisionSchema>;

export default function ReviewerDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [detail, setDetail] = useState<PriorAuthDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<DecisionFormValues>({
    resolver: zodResolver(decisionSchema),
    defaultValues: { reviewerNotes: "" },
  });

  const selectedDecision = watch("decision");

  useEffect(() => {
    if (!id) return;
    getPriorAuthDetail(Number(id))
      .then(setDetail)
      .catch(() => setFetchError("Failed to load request details."))
      .finally(() => setLoading(false));
  }, [id]);

  const onSubmit = async (data: DecisionFormValues) => {
    setSubmitError(null);
    try {
      await submitReviewDecision(
        Number(id),
        data.decision,
        data.reviewerNotes.trim() || null
      );
      navigate("/reviewer");
    } catch {
      setSubmitError("Failed to submit decision. Please try again.");
    }
  };

  if (loading) {
    return (
      <div className="flex min-h-svh items-center justify-center text-sm text-muted-foreground">
        <Loader2 className="mr-2 size-4 animate-spin" />
        Loading request...
      </div>
    );
  }

  if (fetchError || !detail) {
    return (
      <div className="min-h-svh bg-background p-6">
        <p className="text-sm text-destructive">{fetchError ?? "Request not found."}</p>
      </div>
    );
  }

  const clinicalData = JSON.parse(detail.clinicalData || "{}") as Record<string, unknown>;
  const clinicalEntries = Object.entries(clinicalData).filter(
    ([, value]) => value !== "" && value !== null && value !== undefined
  );

  return (
    <div className="min-h-svh bg-background px-4 py-6 sm:px-6">
      <div className="mx-auto max-w-3xl space-y-6">
        <Button variant="ghost" size="sm" onClick={() => navigate("/reviewer")}>
          <ArrowLeft className="size-4" />
          Back
        </Button>

        <Card>
          <CardHeader>
            <CardTitle>Request Summary</CardTitle>
            <CardDescription>
              Clinical information submitted for this authorization request.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <dl className="grid grid-cols-2 gap-x-6 gap-y-3 text-sm">
              <div>
                <dt className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                  Patient
                </dt>
                <dd className="mt-0.5 font-medium">{detail.patientName}</dd>
              </div>
              <div>
                <dt className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                  Practitioner
                </dt>
                <dd className="mt-0.5 font-medium">{detail.practitionerName}</dd>
              </div>
              <div>
                <dt className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                  Service
                </dt>
                <dd className="mt-0.5">
                  <span className="font-medium">{detail.serviceCode}</span>
                  <span className="ml-1.5 text-muted-foreground">{detail.serviceCodeDisplay}</span>
                </dd>
              </div>
              <div>
                <dt className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                  Submitted
                </dt>
                <dd className="mt-0.5">{formatDate(detail.createdAt)}</dd>
              </div>
            </dl>

            {clinicalEntries.length > 0 && (
              <div className="border-t pt-4">
                <p className="mb-3 text-xs font-medium uppercase tracking-wide text-muted-foreground">
                  Clinical Criteria
                </p>
                <dl className="space-y-2">
                  {clinicalEntries.map(([key, value]) => (
                    <div key={key} className="flex gap-2 text-sm">
                      <dt className="w-56 shrink-0 text-muted-foreground">{getFieldLabel(key)}</dt>
                      <dd className="font-medium">{formatClinicalValue(value)}</dd>
                    </div>
                  ))}
                </dl>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Reviewer Decision</CardTitle>
            <CardDescription>Select a decision and provide notes if denying.</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="flex gap-3">
                <button
                  type="button"
                  onClick={() => setValue("decision", "Approved", { shouldValidate: true })}
                  className={cn(
                    "flex h-10 flex-1 items-center justify-center gap-2 rounded-md border text-sm font-medium transition-colors",
                    selectedDecision === "Approved"
                      ? "border-green-600 bg-green-600/10 text-green-700 dark:text-green-400"
                      : "border-border bg-background text-muted-foreground hover:bg-muted"
                  )}
                >
                  <CheckCircle2 className="size-4" />
                  Approve
                </button>
                <button
                  type="button"
                  onClick={() => setValue("decision", "Denied", { shouldValidate: true })}
                  className={cn(
                    "flex h-10 flex-1 items-center justify-center gap-2 rounded-md border text-sm font-medium transition-colors",
                    selectedDecision === "Denied"
                      ? "border-destructive bg-destructive/10 text-destructive"
                      : "border-border bg-background text-muted-foreground hover:bg-muted"
                  )}
                >
                  <XCircle className="size-4" />
                  Deny
                </button>
              </div>
              {errors.decision && (
                <p className="text-xs text-destructive">{errors.decision.message}</p>
              )}

              <div className="space-y-2">
                <Label htmlFor="reviewerNotes">Reviewer Notes</Label>
                <textarea
                  id="reviewerNotes"
                  rows={4}
                  placeholder={
                    selectedDecision === "Denied"
                      ? "Required when denying..."
                      : "Optional notes..."
                  }
                  className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  aria-invalid={!!errors.reviewerNotes}
                  {...register("reviewerNotes")}
                />
                {errors.reviewerNotes && (
                  <p className="text-xs text-destructive">{errors.reviewerNotes.message}</p>
                )}
              </div>

              {submitError && (
                <p className="text-xs text-destructive">{submitError}</p>
              )}

              <div className="flex justify-end">
                <Button type="submit" disabled={isSubmitting || !selectedDecision}>
                  {isSubmitting ? (
                    <>
                      <Loader2 className="size-4 animate-spin" />
                      Submitting...
                    </>
                  ) : (
                    "Submit Decision"
                  )}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
