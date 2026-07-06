param location string
param suffix string
param appInsightsConnectionString string
param sqlConnectionString string
param serviceBusNamespaceName string
param serviceBusSenderRuleName string

// Resolved here (same deployment) so the connection string never crosses a
// module boundary as an output, which would persist it in deployment history.
resource senderRule 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' existing = {
  name: '${serviceBusNamespaceName}/${serviceBusSenderRuleName}'
}

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'plan-${suffix}'
  location: location
  kind: 'linux'
  sku: {
    name: 'B1'
  }
  properties: {
    reserved: true
  }
}

resource api 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-${suffix}'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      healthCheckPath: '/health'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: sqlConnectionString
        }
        {
          name: 'ConnectionStrings__ServiceBus'
          value: senderRule.listKeys().primaryConnectionString
        }
        {
          name: 'Outbox__PollIntervalSeconds'
          value: '5'
        }
      ]
    }
  }
}

output appName string = api.name
output hostname string = api.properties.defaultHostName
output principalId string = api.identity.principalId
