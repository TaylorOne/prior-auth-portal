import React, { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { CheckCircle2, ChevronDown, ChevronRight, Clock3, FileText, Plus } from "lucide-react";
import { getPriorAuthRequests } from "../api/PriorAuthApi";
import { getFieldLabel } from "@/lib/fieldLabels";
import type { PriorAuthSummary } from "../types/PriorAuth";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

const statusVariant: Record<string, "default" | "secondary" | "destructive"> = {
  Draft: "secondary",
  Submitted: "default",
  UnderReview: "default",
  Approved: "default",
  Denied: "destructive",
};

const statusLabel: Record<string, string> = {
  Draft: "Draft",
  Submitted: "Submitted",
  UnderReview: "Pending Reviewer Decision",
  Approved: "Approved",
  Denied: "Denied",
  NeedsMoreInfo: "More Info Required",
  Cancelled: "Cancelled",
};

function formatDate(dateStr: string | null): string {
  if (!dateStr) return "—";
  return new Date(dateStr).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

export default function PrescriberDashboard() {
  const location = useLocation();
  const navigate = useNavigate();
  const [requests, setRequests] = useState<PriorAuthSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showSubmittedToast, setShowSubmittedToast] = useState(false);
  const [expandedRowId, setExpandedRowId] = useState<number | null>(null);

  useEffect(() => {
    getPriorAuthRequests()
      .then((data) => {
        console.log("[PrescriberDashboard] prior auth requests:", data);
        setRequests(data);
      })
      .catch(() => setError("Could not load prior authorization requests."))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    if (!location.state?.priorAuthSubmitted) return;

    setShowSubmittedToast(true);
    navigate(location.pathname, { replace: true, state: null });
  }, [location.pathname, location.state, navigate]);

  useEffect(() => {
    if (!showSubmittedToast) return;

    const timeoutId = window.setTimeout(() => {
      setShowSubmittedToast(false);
    }, 4000);

    return () => window.clearTimeout(timeoutId);
  }, [showSubmittedToast]);

  return (
    <div className="min-h-svh bg-background px-4 py-6 text-foreground sm:px-6">
      {showSubmittedToast && (
        <div className="fixed right-6 top-6 z-50 rounded-md border bg-background px-4 py-3 text-sm font-medium text-foreground shadow-md">
          Prior auth request submitted successfully.
        </div>
      )}

      <div className="mx-auto max-w-6xl space-y-6">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div className="space-y-1.5">
            <h1 className="m-0 text-2xl font-semibold tracking-normal">
              Prior Authorization Requests
            </h1>
            <p className="text-sm text-muted-foreground">
              Manage and track submitted authorization requests.
            </p>
          </div>
          <Button onClick={() => navigate("/submit")}>
            <Plus className="size-4" />
            New Request
          </Button>
        </div>

        <div className="grid gap-4 sm:grid-cols-3">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-1">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Total Requests
              </CardTitle>
              <FileText className="size-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-semibold">{requests.length}</p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-1">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Pending Review
              </CardTitle>
              <Clock3 className="size-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-semibold">
                {requests.filter((r) => r.status === "Submitted").length}
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-1">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Approved
              </CardTitle>
              <CheckCircle2 className="size-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-semibold">
                {requests.filter((r) => r.status === "Approved").length}
              </p>
            </CardContent>
          </Card>
        </div>

        <Card>
          <CardContent className="pt-4">
            {loading && (
              <p className="text-sm text-muted-foreground">Loading requests...</p>
            )}
            {error && <p className="text-sm text-destructive">{error}</p>}
            {!loading && !error && (
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Patient</TableHead>
                      <TableHead>Service/Medication</TableHead>
                      <TableHead>Practitioner</TableHead>
                      <TableHead>Priority</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>Submitted</TableHead>
                      <TableHead className="w-8" />
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {requests.map((r) => {
                      const isDenied = r.status === "Denied";
                      const isExpanded = expandedRowId === r.id;
                      const denialReasons: { Field: string; FailureReason: string }[] =
                        isDenied && r.evaluationReason
                          ? JSON.parse(r.evaluationReason)
                          : [];

                      return (
                        <React.Fragment key={r.id}>
                          <TableRow
                            className={isDenied ? "cursor-pointer" : undefined}
                            onClick={isDenied ? () => setExpandedRowId(isExpanded ? null : r.id) : undefined}
                          >
                            <TableCell className="font-medium">{r.patientName}</TableCell>
                            <TableCell>
                              <span className="font-medium">{r.serviceCode}</span>
                              <span className="block text-xs text-muted-foreground">
                                {r.serviceCodeDisplay}
                              </span>
                            </TableCell>
                            <TableCell>
                              <span>{r.practitionerName}</span>
                              <span className="block text-xs text-muted-foreground">
                                {r.specialty}
                              </span>
                            </TableCell>
                            <TableCell className="capitalize">{r.priority}</TableCell>
                            <TableCell>
                              <Badge variant={statusVariant[r.status] ?? "default"}>
                                {statusLabel[r.status] ?? r.status}
                              </Badge>
                            </TableCell>
                            <TableCell>{formatDate(r.createdAt)}</TableCell>
                            <TableCell className="w-8 text-muted-foreground">
                              {isDenied && (
                                isExpanded
                                  ? <ChevronDown className="size-4" />
                                  : <ChevronRight className="size-4" />
                              )}
                            </TableCell>
                          </TableRow>
                          {isDenied && isExpanded && (
                            <TableRow key={`${r.id}-detail`} className="hover:bg-transparent">
                              <TableCell colSpan={7} className="bg-muted/50 px-6 py-4">
                                <div className="flex items-start gap-8">
                                  <p className="shrink-0 text-xs font-semibold uppercase tracking-wide text-destructive">
                                    Denial Reasons
                                  </p>
                                  {denialReasons.length > 0 ? (
                                    <ul className="space-y-1">
                                      {denialReasons.map((reason, i) => (
                                        <li key={i} className="text-sm text-foreground">
                                          {reason.FailureReason.replace(reason.Field, getFieldLabel(reason.Field))}
                                        </li>
                                      ))}
                                    </ul>
                                  ) : (
                                    <p className="text-sm text-muted-foreground">No details available.</p>
                                  )}
                                </div>
                              </TableCell>
                            </TableRow>
                          )}
                        </React.Fragment>
                      );
                    })}
                  </TableBody>
                </Table>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
