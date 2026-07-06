param location string
param suffix string

// Standard (not Basic) so the queue can deduplicate on MessageId — the outbox
// dispatcher stamps the outbox row id as MessageId precisely for this (ADR-008).
resource namespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: 'sb-${suffix}'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
  }
}

resource queue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: namespace
  name: 'auth-evaluation'
  properties: {
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 5
    deadLetteringOnMessageExpiration: true
    defaultMessageTimeToLive: 'P1D'
    lockDuration: 'PT1M'
  }
}

// Least-privilege rules: the API only sends, the Function App only listens.
resource senderRule 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' = {
  parent: namespace
  name: 'api-send'
  properties: {
    rights: ['Send']
  }
}

resource listenRule 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' = {
  parent: namespace
  name: 'functions-listen'
  properties: {
    rights: ['Listen']
  }
}

output namespaceName string = namespace.name
output queueName string = queue.name
output senderRuleName string = senderRule.name
output listenRuleName string = listenRule.name
