import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { CheckCircle2, Clock3, FileText, Plus } from "lucide-react";
import { getPriorAuthRequests } from "../api/PriorAuthApi";
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

  useEffect(() => {
    getPriorAuthRequests()
      .then(setRequests)
      .catch(() => setError("Could not load prior authorization requests."))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    if (!location.state?.priorAuthSubmitted) return;

    setShowSubmittedToast(true);
    navigate(location.pathname, { replace: true, state: null });

    const timeoutId = window.setTimeout(() => {
      setShowSubmittedToast(false);
    }, 4000);

    return () => window.clearTimeout(timeoutId);
  }, [location.pathname, location.state, navigate]);

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
                      <TableHead>Determination</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {requests.map((r) => (
                      <TableRow key={r.id}>
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
                        <TableCell>{formatDate(r.determinationDate)}</TableCell>
                      </TableRow>
                    ))}
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
