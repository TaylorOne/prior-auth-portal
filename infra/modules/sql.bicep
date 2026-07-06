param location string
param suffix string
param entraAdminLogin string
param entraAdminObjectId string

// Entra-only authentication: no SQL passwords exist anywhere in this stack.
// App identities are added as database users via T-SQL after deployment
// (CREATE USER [app-name] FROM EXTERNAL PROVIDER) — see infra/README.md.
resource server 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: 'sql-${suffix}'
  location: location
  properties: {
    minimalTlsVersion: '1.2'
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: entraAdminLogin
      sid: entraAdminObjectId
      tenantId: tenant().tenantId
      principalType: 'Group'
    }
  }
}

// Serverless with auto-pause: near-zero cost when idle. The API's
// DatabaseWarmupService and TransientLoginRetryStrategy exist to absorb
// the resume latency this tier introduces.
resource database 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: server
  name: 'sqldb-${suffix}'
  location: location
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    autoPauseDelay: 60
    minCapacity: json('0.5')
    maxSizeBytes: 34359738368
    zoneRedundant: false
  }
}

// Demo-friendly: allow Azure services (App Service, Functions) through the
// server firewall. Private endpoints would replace this in a hardened setup.
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: server
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

output serverFqdn string = server.properties.fullyQualifiedDomainName
output databaseName string = database.name
output managedIdentityConnectionString string = 'Server=tcp:${server.properties.fullyQualifiedDomainName},1433;Initial Catalog=${database.name};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
