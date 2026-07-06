using 'main.bicep'

param baseName = 'priorauth'
param environmentName = 'dev'

// Entra group (or user) that becomes the SQL server administrator.
// Replace with your own group's display name and object id:
//   az ad group show --group sql-admins --query id -o tsv
param sqlEntraAdminLogin = 'sql-admins'
param sqlEntraAdminObjectId = '00000000-0000-0000-0000-000000000000'
