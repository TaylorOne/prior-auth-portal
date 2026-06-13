import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Loader2 } from "lucide-react";
import { getReviewQueue } from "../api/PriorAuthApi";
import type { ReviewQueueItem } from "@/types/ReviewQueue";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

export default function ReviewerDashboard() {
  const navigate = useNavigate();
  const [queue, setQueue] = useState<ReviewQueueItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getReviewQueue()
      .then(setQueue)
      .catch(() => setError("Failed to load review queue."))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="flex min-h-svh items-center justify-center text-sm text-muted-foreground">
        <Loader2 className="mr-2 size-4 animate-spin" />
        Loading review queue...
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-svh bg-background p-6">
        <p className="text-sm text-destructive">{error}</p>
      </div>
    );
  }

  return (
    <div className="min-h-svh bg-background px-4 py-6 sm:px-6">
      <div className="mx-auto max-w-5xl space-y-6">
        <div>
          <h1 className="text-2xl font-semibold tracking-normal">Review Queue</h1>
          <p className="text-sm text-muted-foreground">Requests pending manual clinical review.</p>
        </div>
        <Card>
          <CardHeader>
            <CardTitle>Pending Requests</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            {queue.length === 0 ? (
              <p className="px-6 py-8 text-center text-sm text-muted-foreground">
                No requests pending review
              </p>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Patient</TableHead>
                    <TableHead>Service</TableHead>
                    <TableHead>Practitioner</TableHead>
                    <TableHead>Submitted</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {queue.map((item) => (
                    <TableRow
                      key={item.id}
                      className="cursor-pointer"
                      onClick={() => navigate(`/reviewer/${item.id}`)}
                    >
                      <TableCell className="font-medium">{item.patientName}</TableCell>
                      <TableCell>
                        <span className="font-medium">{item.serviceCode}</span>
                        <span className="block text-xs text-muted-foreground">
                          {item.serviceCodeDisplay}
                        </span>
                      </TableCell>
                      <TableCell>{item.practitionerName}</TableCell>
                      <TableCell>{formatDate(item.createdAt)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
