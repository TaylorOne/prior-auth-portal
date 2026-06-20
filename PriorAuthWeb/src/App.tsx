import { useEffect, useState, type ReactNode } from "react";
import { Routes, Route, Navigate, useNavigate } from "react-router-dom";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { InteractionRequiredAuthError, InteractionStatus } from "@azure/msal-browser";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { LogOut, ShieldAlert } from "lucide-react";
import PrescriberDashboard from "./pages/PrescriberDashboard";
import SubmitPARequest from "./pages/SubmitPARequest";
import ReviewerDashboard from "./pages/ReviewerDashboard";
import ReviewerDetail from "./pages/ReviewerDetail";
import Login from "./pages/Login";
import { apiRequest } from "./auth/msalConfig";
import './App.css'

function RequireAuth({ children }: { children: ReactNode }) {
  const { instance } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const account = instance.getActiveAccount() ?? instance.getAllAccounts()[0];

  function handleLogout() {
    void instance.logoutRedirect({
      account,
      postLogoutRedirectUri: window.location.origin,
    });
  }

  return isAuthenticated ? (
    <>
      <div className="fixed bottom-4 right-4 z-50">
        <Button type="button" variant="outline" size="sm" onClick={handleLogout}>
          <LogOut className="size-4" />
          Sign out
        </Button>
      </div>
      {children}
    </>
  ) : <Navigate to="/login" replace />;
}

function readRoles(value: unknown): string[] {
  if (Array.isArray(value)) return value.filter((role): role is string => typeof role === "string");
  return typeof value === "string" ? [value] : [];
}

function readAccessTokenRoles(accessToken: string): string[] {
  const payload = accessToken.split(".")[1];
  if (!payload) return [];

  try {
    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64.padEnd(base64.length + ((4 - base64.length % 4) % 4), "=");
    const claims = JSON.parse(window.atob(padded)) as Record<string, unknown>;
    return readRoles(claims.roles);
  } catch {
    return [];
  }
}

function MissingRole({ error }: { error: string | null }) {
  const { instance } = useMsal();
  const account = instance.getActiveAccount() ?? instance.getAllAccounts()[0];

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Access role required</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4 text-sm text-muted-foreground">
          <p>
            {account?.username ?? "This account"} is signed in, but no
            Prescriber or Reviewer app role was found for the API token.
          </p>
          {error && <p>{error}</p>}
          <Button type="button" variant="outline" onClick={() => instance.logoutRedirect()}>
            Sign out
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}

function useCurrentRoles() {
  const { instance, inProgress } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const [roles, setRoles] = useState<string[] | null>(null);
  const [roleError, setRoleError] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated || inProgress !== InteractionStatus.None) return;

    let cancelled = false;

    async function loadRoles() {
      const account = instance.getActiveAccount() ?? instance.getAllAccounts()[0];
      if (!account) {
        if (!cancelled) setRoles([]);
        return;
      }

      instance.setActiveAccount(account);

      const idTokenRoles = readRoles(account.idTokenClaims?.roles);
      if (idTokenRoles.length > 0) {
        if (!cancelled) setRoles(idTokenRoles);
        return;
      }

      try {
        const result = await instance.acquireTokenSilent({ ...apiRequest, account });
        if (!cancelled) setRoles(readAccessTokenRoles(result.accessToken));
      } catch (error) {
        if (error instanceof InteractionRequiredAuthError) {
          await instance.acquireTokenRedirect(apiRequest);
          return;
        }

        console.error("Unable to read roles from API access token", error);
        if (!cancelled) {
          setRoleError("The API access token could not be acquired silently.");
          setRoles([]);
        }
      }
    }

    void loadRoles();

    return () => {
      cancelled = true;
    };
  }, [inProgress, instance, isAuthenticated]);

  return { inProgress, isAuthenticated, roleError, roles };
}

function RoleRedirect() {
  const { inProgress, isAuthenticated, roleError, roles } = useCurrentRoles();

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (inProgress !== InteractionStatus.None || roles === null) return null;

  if (roles.includes("Reviewer")) return <Navigate to="/reviewer" replace />;
  if (roles.includes("Prescriber")) return <Navigate to="/dashboard" replace />;

  return <MissingRole error={roleError} />;
}

function RoleRequired({
  children,
  role,
}: {
  children: ReactNode;
  role: "Prescriber" | "Reviewer";
}) {
  const { instance } = useMsal();
  const navigate = useNavigate();
  const { inProgress, isAuthenticated, roles } = useCurrentRoles();
  const account = instance.getActiveAccount() ?? instance.getAllAccounts()[0];
  const otherRole = role === "Reviewer" ? "Prescriber" : "Reviewer";
  const fallbackPath = roles?.includes("Prescriber") ? "/dashboard" : "/reviewer";

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (inProgress !== InteractionStatus.None || roles === null) return null;

  if (roles.includes(role)) {
    return <RequireAuth>{children}</RequireAuth>;
  }

  return (
    <RequireAuth>
      <div className="min-h-svh bg-background px-4 py-6 sm:px-6">
        <div className="mx-auto flex min-h-[70svh] max-w-md items-center">
          <Card className="w-full">
            <CardHeader>
              <div className="mb-2 flex size-10 items-center justify-center rounded-md bg-muted text-muted-foreground">
                <ShieldAlert className="size-5" />
              </div>
              <CardTitle>{role} access required</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-sm text-muted-foreground">
              <p>
                {account?.username ?? "This account"} is signed in, but this page is only
                available to users with the {role} app role.
              </p>
              {roles.includes(otherRole) && (
                <p className="pt-2">
                  You are currently signed in as a {otherRole}. Sign out and choose a
                  {role} test account to view this area.
                </p>
              )}
              <div className="flex flex-wrap gap-2 pt-6">
                <Button type="button" variant="outline" onClick={() => instance.logoutRedirect()}>
                  <LogOut className="size-4" />
                  Sign out
                </Button>
                {(roles.includes("Prescriber") || roles.includes("Reviewer")) && (
                  <Button type="button" onClick={() => navigate(fallbackPath)}>
                    Go to my dashboard
                  </Button>
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </RequireAuth>
  );
}

export default function App() {
  const { inProgress } = useMsal();

  // Block rendering entirely while MSAL is processing 
  // the redirect response — prevents components from
  // mounting and firing API calls mid-handshake
  if (
    inProgress === InteractionStatus.Startup ||
    inProgress === InteractionStatus.HandleRedirect
  ) {
    return null; // or a small loading spinner if you prefer
  }

  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/" element={<RoleRedirect />} />
      <Route path="/dashboard" element={<RoleRequired role="Prescriber"><PrescriberDashboard /></RoleRequired>} />
      <Route path="/submit" element={<RoleRequired role="Prescriber"><SubmitPARequest /></RoleRequired>} />
      <Route path="/reviewer" element={<RoleRequired role="Reviewer"><ReviewerDashboard /></RoleRequired>} />
      <Route path="/reviewer/:id" element={<RoleRequired role="Reviewer"><ReviewerDetail /></RoleRequired>} />
    </Routes>
  );
}
