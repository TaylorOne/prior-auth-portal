import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { Navigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { FileText } from "lucide-react";
import { loginRequest } from "@/auth/msalConfig";

export default function Login() {
  const { instance } = useMsal();
  const isAuthenticated = useIsAuthenticated();

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  function handleLogin() {
    instance.loginRedirect(loginRequest);
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <Card className="w-full max-w-sm">
        <CardHeader className="text-center">
          <div className="flex justify-center mb-2">
            <FileText className="h-8 w-8 text-blue-600" />
          </div>
          <CardTitle className="text-xl">Prior Auth Portal</CardTitle>
          <p className="text-sm text-muted-foreground">Sign in with your organization account</p>
        </CardHeader>
        <CardContent>
          <Button className="w-full" onClick={handleLogin}>
            Sign in with Microsoft
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
