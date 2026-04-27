# ADR-007: Deferred Outbox Pattern for Service Bus Delivery

## Status
Acknowledged — not implemented

## Context
The POST /priorauth endpoint persists a PriorAuthRequest to SQL Server and then
publishes a PriorAuthSubmittedMessage to Azure Service Bus. These are two separate
I/O operations with no distributed transaction spanning them. If SaveChangesAsync
succeeds but SendMessageAsync fails, the request exists in the database but no
evaluation message is ever delivered. The request silently stalls in Draft status
with no evaluation triggered.

## Decision
The outbox pattern is not implemented in this project. The two operations remain
non-atomic.

## Rationale
The transactional outbox pattern would require:
- An OutboxMessage table persisted in the same transaction as the request
- A background process (hosted service or Azure Function) polling the outbox and
  delivering messages to the bus
- Idempotency handling on the consumer side to guard against duplicate delivery

This is meaningful infrastructure overhead. For a portfolio project demonstrating
the Service Bus integration pattern, the architectural tradeoff is acknowledged and
documented rather than implemented. The failure mode (stuck Draft request) is
acceptable in a demo context where data integrity is not production-critical.

## Consequences
- A Service Bus send failure leaves a PriorAuthRequest in Draft status permanently
  with no evaluation triggered and no visibility into the failure
- In production this would require either the outbox pattern or at minimum a
  compensating mechanism — a scheduled job scanning for Draft requests older than
  a threshold and re-queuing them
- CMS-0057-F real-time adjudication requirements would make this failure mode
  unacceptable in a production payer system