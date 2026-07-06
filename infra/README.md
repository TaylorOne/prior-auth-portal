# Infrastructure as Code

Bicep templates describing the complete Azure environment for the Prior Auth
Portal. One `az deployment group create` produces a working, uniquely-named
stack:

| Module | Resources |
|---|---|
| `modules/monitoring.bicep` | Log Analytics workspace, Application Insights (workspace-based) |
| `modules/servicebus.bicep` | Service Bus namespace (Standard), `auth-evaluation` queue with duplicate detection, least-privilege Send/Listen authorization rules |
| `modules/sql.bicep` | SQL logical server (Entra-only auth, no SQL passwords anywhere), serverless database with auto-pause |
| `modules/appservice.bicep` | Linux App Service plan (B1) + API web app with system-assigned identity |
| `modules/functions.bicep` | Storage account, Linux consumption plan, .NET 9 isolated Function App |
| `modules/staticwebapp.bicep` | Static Web App (Free) for the React frontend |

Resource names embed `uniqueString(resourceGroup().id)`, so deployments are
idempotent within a resource group and never collide with the original
hand-created resources (`app-prior-auth-dev`, `func-prior-auth-dev`, …).
This stack runs **in parallel** to those until you choose to cut over.

## Deploy

### From your machine

```bash
az group create --name rg-priorauth-dev --location eastus2

az deployment group create \
  --resource-group rg-priorauth-dev \
  --template-file infra/main.bicep \
  --parameters infra/main.dev.bicepparam \
  --parameters sqlEntraAdminObjectId=$(az ad group show --group sql-admins --query id -o tsv)
```

Preview changes first with `az deployment group what-if` (same arguments).

### From GitHub Actions

The `Infrastructure` workflow compiles and lints the templates on every push
that touches `infra/`. Deployment is **manual only**: run the workflow via
*Actions → Infrastructure → Run workflow*, choose the resource group and
`what-if` (preview) or `deploy`. It reuses the existing OIDC secrets
(`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` — see ADR-007)
plus two new ones:

- `AZURE_SQL_ADMIN_LOGIN` — display name of the Entra group that administers SQL
- `AZURE_SQL_ADMIN_OBJECT_ID` — that group's object id

## Post-deployment steps

Some wiring cannot be expressed in ARM/Bicep:

1. **SQL database users for the app identities.** Entra server admins are set
   by the template, but database users are T-SQL. Connect to the new database
   as the Entra admin and run (substitute the deployed site names from the
   deployment outputs):

   ```sql
   CREATE USER [app-priorauth-dev-<token>] FROM EXTERNAL PROVIDER;
   ALTER ROLE db_datareader ADD MEMBER [app-priorauth-dev-<token>];
   ALTER ROLE db_datawriter ADD MEMBER [app-priorauth-dev-<token>];

   CREATE USER [func-priorauth-dev-<token>] FROM EXTERNAL PROVIDER;
   ALTER ROLE db_datareader ADD MEMBER [func-priorauth-dev-<token>];
   ALTER ROLE db_datawriter ADD MEMBER [func-priorauth-dev-<token>];
   ```

2. **EF migrations.** Point `ConnectionStrings__DefaultConnection` at the new
   database and run `dotnet ef database update` (the CI/CD deploy job already
   does this for the original environment).

3. **Frontend + auth config.** Add the new Static Web App hostname to the
   API's CORS policy (`PriorAuthApi/Program.cs`) and to the Entra app
   registration's redirect URIs, and set the `VITE_*` build variables for the
   new API URL.

## Teardown

Everything lives in one resource group:

```bash
az group delete --name rg-priorauth-dev
```

## Known simplifications

- Service Bus and storage are wired with connection strings (matching the
  current application code); moving both to managed identity + RBAC is the
  natural next hardening step.
- The SQL firewall allows all Azure services (`0.0.0.0` rule) rather than
  VNet integration / private endpoints.
- App settings live on the sites directly; a Key Vault with references would
  centralize them.
