# ADR-008: Transactional Outbox Pattern for Service Bus Delivery

## Status
Implemented (previously: acknowledged — not implemented)

## Context
The POST /priorauth endpoint persists a PriorAuthRequest to SQL Server and needs a
PriorAuthSubmittedMessage published to Azure Service Bus so the evaluation function
runs. These are two separate I/O operations with no distributed transaction spanning
them. In the original implementation the endpoint called SendMessageAsync directly
after SaveChangesAsync: if the send failed, the request existed in the database but
no evaluation message was ever delivered, and the request silently stalled with no
evaluation triggered and no visibility into the failure.

## Decision
Implement the transactional outbox pattern:

- **OutboxMessages table** — the endpoint writes the serialized message to an
  `OutboxMessages` row in the *same database transaction* as the PriorAuthRequest
  (wrapped in the retrying execution strategy). A stable correlation id and
  `verifySucceeded` query detect when a commit succeeded but its acknowledgment
  was lost, preventing the strategy from inserting a duplicate request. Either
  both rows commit or neither does.
- **Background dispatcher** — `OutboxDispatcher`, driven by the
  `OutboxDispatcherService` hosted service in the API process, polls for
  unprocessed rows (default every 5 seconds, configurable via
  `Outbox:PollIntervalSeconds`), sends each to the `auth-evaluation` queue, and
  stamps `ProcessedAt`. Failed sends record `AttemptCount`/`LastError` and are
  retried after a one-minute backoff, indefinitely — a poisoned row stays visible
  in the table rather than being dropped. A filtered index on `ProcessedAt IS NULL`
  keeps the polling query cheap as delivered rows accumulate. The hosted service is
  only registered when a Service Bus connection string is configured.
- **At-least-once delivery** — a crash between the send and the `ProcessedAt`
  update re-sends the message on the next pass. This is acceptable because the
  consumer is idempotent: `AuthEvaluationFunction` skips any request whose status
  is not `Submitted`. The outbox row id is also stamped as the Service Bus
  `MessageId`, so broker-side duplicate detection can be enabled on the queue as a
  second guard.
- The `MessagePublished` audit event is now written by the dispatcher at actual
  delivery time rather than by the endpoint, so the audit trail reflects what
  really happened.

## Consequences
- A Service Bus outage no longer loses evaluations: requests continue to be
  accepted, and queued outbox rows are delivered when the bus recovers.
- Evaluation dispatch gains up to one poll interval of latency compared to the
  previous inline send.
- Delivered rows accumulate in `OutboxMessages`; a retention job (e.g. delete
  processed rows older than 30 days) is a straightforward follow-up if table
  growth ever matters.
- The dispatcher does not claim rows, so multiple API instances could deliver the
  same message concurrently. That only produces duplicate sends — harmless given
  consumer idempotency — but row claiming (UPDLOCK/READPAST) would be the next
  step if exactly-once *dispatch* were ever required.
