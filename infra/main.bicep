// Prior Auth Portal — full environment stack.
// Deploy at resource-group scope:
//   az deployment group create -g <rg> -f infra/main.bicep -p infra/main.dev.bicepparam

@description('Short name used as a prefix for every resource.')
@maxLength(12)
param baseName string = 'priorauth'

@description('Environment discriminator (dev, staging, prod).')
@allowed(['dev', 'staging', 'prod'])
param environmentName string = 'dev'

@description('Region for all resources except the Static Web App.')
param location string = resourceGroup().location

@description('Static Web Apps is only offered in a subset of regions.')
@allowed(['westus2', 'centralus', 'eastus2', 'westeurope', 'eastasia'])
param staticWebAppLocation string = 'eastus2'

@description('Display name of the Entra group or user that administers SQL (e.g. "sql-admins").')
param sqlEntraAdminLogin string

@description('Object id of the Entra group or user that administers SQL.')
param sqlEntraAdminObjectId string

// Globally-unique-name entropy derived from the resource group, so the same
// parameters always produce the same names within one environment.
var resourceToken = toLower(uniqueString(resourceGroup().id, baseName, environmentName))
var suffix = '${baseName}-${environmentName}-${resourceToken}'

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    location: location
    suffix: suffix
  }
}

module serviceBus 'modules/servicebus.bicep' = {
  name: 'service-bus'
  params: {
    location: location
    suffix: suffix
  }
}

module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: {
    location: location
    suffix: suffix
    entraAdminLogin: sqlEntraAdminLogin
    entraAdminObjectId: sqlEntraAdminObjectId
  }
}

module api 'modules/appservice.bicep' = {
  name: 'api'
  params: {
    location: location
    suffix: suffix
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
    sqlConnectionString: sql.outputs.managedIdentityConnectionString
    serviceBusNamespaceName: serviceBus.outputs.namespaceName
    serviceBusSenderRuleName: serviceBus.outputs.senderRuleName
  }
}

module functions 'modules/functions.bicep' = {
  name: 'functions'
  params: {
    location: location
    suffix: suffix
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
    sqlConnectionString: sql.outputs.managedIdentityConnectionString
    serviceBusNamespaceName: serviceBus.outputs.namespaceName
    serviceBusListenRuleName: serviceBus.outputs.listenRuleName
  }
}

module staticWebApp 'modules/staticwebapp.bicep' = {
  name: 'static-web-app'
  params: {
    location: staticWebAppLocation
    suffix: suffix
  }
}

output apiAppName string = api.outputs.appName
output apiHostname string = api.outputs.hostname
output functionAppName string = functions.outputs.appName
output staticWebAppName string = staticWebApp.outputs.appName
output staticWebAppHostname string = staticWebApp.outputs.hostname
output sqlServerFqdn string = sql.outputs.serverFqdn
output sqlDatabaseName string = sql.outputs.databaseName
output serviceBusNamespaceName string = serviceBus.outputs.namespaceName
output appInsightsConnectionString string = monitoring.outputs.appInsightsConnectionString
output apiPrincipalId string = api.outputs.principalId
output functionAppPrincipalId string = functions.outputs.principalId
