import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { PublicClientApplication, EventType } from '@azure/msal-browser'
import type { AuthenticationResult } from '@azure/msal-browser'
import { MsalProvider } from '@azure/msal-react'
import { msalConfig } from './auth/msalConfig'
import { initAuth } from './api/PriorAuthApi'
import './index.css'
import App from './App.tsx'

const msalInstance = new PublicClientApplication(msalConfig)

function setActiveAccount(result: AuthenticationResult | null) {
  const account =
    result?.account ??
    msalInstance.getActiveAccount() ??
    msalInstance.getAllAccounts()[0] ??
    null

  if (account) {
    msalInstance.setActiveAccount(account)
  }
}

msalInstance.addEventCallback((event) => {
  if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
    const account = (event.payload as AuthenticationResult).account
    msalInstance.setActiveAccount(account)
  }
})

await msalInstance.initialize()
const redirectResult = await msalInstance.handleRedirectPromise({
  navigateToLoginRequestUrl: false,
})
setActiveAccount(redirectResult)

initAuth(msalInstance)

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MsalProvider instance={msalInstance}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </MsalProvider>
  </StrictMode>,
)
