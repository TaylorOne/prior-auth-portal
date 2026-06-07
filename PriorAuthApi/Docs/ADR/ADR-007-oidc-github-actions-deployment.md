# ADR-007: Use OIDC Federated Identity for GitHub Actions to Azure Deployment

**Date:** 2026-04-24  
**Status:** Accepted

---

## Context

The CD pipeline requires GitHub Actions to authenticate against Azure in order to deploy the API to App Service. The default approach provided by the `azure/webapps-deploy` action uses a publish profile — an XML blob containing a username and password that is stored as a GitHub Actions secret and submitted on each deployment.

During initial CD pipeline setup, this approach failed with:

```
Publish profile is invalid for app-name and slot-name provided.
```

The root cause was that the App Service had basic authentication disabled at the platform level, which is the secure default for App Services in Azure and a configuration worth preserving.

---

## Decision

Use OIDC (OpenID Connect) federated identity to authenticate GitHub Actions to Azure, replacing the publish profile approach entirely.

The workflow uses `azure/login@v2` with `id-token: write` permissions granted at the workflow level:

```yaml
permissions:
  id-token: write
  contents: read

- name: Azure Login (OIDC)
  uses: azure/login@v2
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

- name: Deploy to Azure App Service
  uses: azure/webapps-deploy@v3
  with:
    app-name: app-prior-auth-dev
    package: ./publish
```

---

## Alternatives Considered

**Publish profile (basic auth)**  
The default approach. Stores a username and password as a GitHub secret and submits them on each deployment. Rejected because it required re-enabling basic authentication on the App Service, which weakens the security posture without meaningful benefit. Basic auth credentials are also long-lived and static — a leaked secret remains valid until manually rotated.

**Service principal with client secret**  
A more established approach where an Azure AD service principal is created with a client secret stored as a GitHub secret. More portable than a publish profile but still relies on a long-lived static credential. OIDC is strictly preferable where supported since it eliminates the credential entirely.

---

## Reasons

- **Basic auth disabled is a sound default.** Re-enabling it to unblock the pipeline would trade a deployment convenience for a permanent reduction in the App Service security posture. OIDC lets the pipeline work without compromising that setting.
- **OIDC uses short-lived tokens.** Rather than a stored credential, GitHub and Azure negotiate a trust relationship at runtime using a token that is scoped to the specific workflow run and expires immediately after. There is no secret that can leak, expire unexpectedly, or require manual rotation.
- **Aligns with Microsoft's recommended approach.** Azure's own documentation recommends OIDC over service principal secrets for CI/CD pipelines as of 2023. Using the recommended path reduces long-term maintenance burden.
- **Secrets stored are non-sensitive identifiers.** `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, and `AZURE_SUBSCRIPTION_ID` are identifiers, not credentials. Exposure of these values alone is not sufficient to authenticate against Azure.

---

## Consequences

- Basic authentication remains disabled on the App Service, preserving the secure default.
- Three GitHub secrets are required (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`) rather than one, but none of these are credentials.
- An OIDC federated identity credential must be configured on the Azure AD app registration, scoped to the specific GitHub repo and branch (`main`). This is a one-time setup step that must be documented for anyone reproducing the environment.
- The publish profile secret (`AZURE_WEBAPP_PUBLISH_PROFILE`) can be removed from GitHub secrets entirely as it is no longer used.