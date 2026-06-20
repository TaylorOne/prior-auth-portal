import { LogLevel, type Configuration, type RedirectRequest } from "@azure/msal-browser";

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID}`,
    redirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: "sessionStorage",
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (!containsPii) console.log(`[MSAL][${LogLevel[level]}] ${message}`);
      },
      logLevel: LogLevel.Error,
    }
  }
};

export const apiRequest: Pick<RedirectRequest, "scopes"> = {
  scopes: [`api://${import.meta.env.VITE_AZURE_API_CLIENT_ID}/access_as_user`],
};

export const loginRequest: RedirectRequest = {
  ...apiRequest,
  prompt: "select_account",
};
