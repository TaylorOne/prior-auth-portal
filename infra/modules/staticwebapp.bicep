param location string
param suffix string

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: 'stapp-${suffix}'
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    // Deployed via GitHub Actions (deploy-frontend.yml), not SWA's built-in CI.
    provider: 'Custom'
  }
}

output appName string = staticWebApp.name
output hostname string = staticWebApp.properties.defaultHostname
