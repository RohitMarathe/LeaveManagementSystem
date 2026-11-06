# Outbox Publisher setup

## SQL deployment order

Run these scripts against the Leave Management database, in this order:

1. `SQL/Tables/OutboxMessages.sql`
2. `SQL/Sp/usp_SubmitLeaveRequestWithOutbox.sql`
3. `SQL/Sp/usp_ClaimOutboxMessages.sql`
4. `SQL/Sp/usp_MarkOutboxMessagePublished.sql`
5. `SQL/Sp/usp_ScheduleOutboxMessageRetry.sql`

The table script is safe to re-run. If the original Outbox table already exists, it adds the publisher lease and terminal-failure columns and refreshes the publishing-status index.

## Function App settings

Configure these application settings on `func-leavemanagement-dev`:

| Setting | Value |
|---|---|
| `FUNCTIONS_WORKER_RUNTIME` | `dotnet-isolated` |
| `OutboxPublisherSchedule` | `*/30 * * * * *` |
| `OutboxPublisherBatchSize` | `50` |
| `ServiceBus__FullyQualifiedNamespace` | `sb-leavemanagement-dev.servicebus.windows.net` |
| `ConnectionStrings__DefaultConnection` | Secure connection string for the Leave Management Azure SQL database |

Do not commit the SQL connection string or any credentials to source control. Use the Function App configuration or a secret store.

The Function App needs:
- Its existing system-assigned Managed Identity.
- `Azure Service Bus Data Sender` on the Service Bus namespace.
- Network access to the Azure SQL server and database permissions to execute the Outbox stored procedures and read/update `dbo.OutboxMessages`.
- Its `AzureWebJobsStorage` setting for Timer Trigger scheduling.

The API's existing `ServiceBusConnection__fullyQualifiedNamespace` setting is for the Service Bus-trigger configuration. The publisher's client uses `ServiceBus__FullyQualifiedNamespace` as shown above.

## Publisher behavior

- Runs every 30 seconds by default.
- Claims up to 50 due records per run.
- A five-minute SQL lease prevents concurrent publisher runs from claiming the same rows.
- Publishes to the `leave-events` Topic.
- Uses the Outbox row ID as the Service Bus `MessageId`.
- Retries failures with increasing delays.
- Stops automatic retries after 10 failed attempts and sets `FailedAtUtc` for operational review.
- A send can succeed while the SQL completion update fails. That can cause a duplicate delivery after the lease expires. Consumers must therefore remain idempotent.

## Monitoring queries

Pending or currently leased events:

```sql
SELECT Id, EventType, Attempts, NextAttemptAtUtc, LockId, LockedUntilUtc, LastError
FROM dbo.OutboxMessages
WHERE ProcessedAtUtc IS NULL
  AND FailedAtUtc IS NULL
ORDER BY CreatedAtUtc;
```

Successfully published events:

```sql
SELECT Id, EventType, OccurredAtUtc, ProcessedAtUtc, Attempts
FROM dbo.OutboxMessages
WHERE ProcessedAtUtc IS NOT NULL
ORDER BY ProcessedAtUtc DESC;
```

Events requiring operational review after the retry limit:

```sql
SELECT Id, EventType, Attempts, FailedAtUtc, LastError
FROM dbo.OutboxMessages
WHERE FailedAtUtc IS NOT NULL
ORDER BY FailedAtUtc DESC;
```
