import { useEffect, useState } from "react";
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
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

const statusVariant: Record<string, "default" | "secondary" | "destructive"> = {
  Draft: "secondary",
  Submitted: "default",
  Approved: "default",
  Denied: "destructive",
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
  const [requests, setRequests] = useState<PriorAuthSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getPriorAuthRequests()
      .then(setRequests)
      .catch(() => setError("Could not load prior authorization requests."))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="p-6 max-w-6xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Prior Authorization Requests</h1>
        <p className="text-muted-foreground text-sm mt-1">
          Manage and track submitted authorization requests
        </p>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <Card>
          <CardHeader className="pb-1">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Requests
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">{requests.length}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-1">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Pending Review
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">
              {requests.filter((r) => r.status === "Submitted").length}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-1">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Approved
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-3xl font-bold">
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
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>ID</TableHead>
                  <TableHead>Patient</TableHead>
                  <TableHead>Service</TableHead>
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
                    <TableCell className="font-mono text-sm">#{r.id}</TableCell>
                    <TableCell>{r.patientName}</TableCell>
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
                        {r.status}
                      </Badge>
                    </TableCell>
                    <TableCell>{formatDate(r.createdAt)}</TableCell>
                    <TableCell>{formatDate(r.determinationDate)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}