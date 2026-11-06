/*
    Stored Procedure: dbo.usp_ScheduleOutboxMessageRetry
    Purpose: Record a failed publish attempt and release its lease.
             After 10 failed attempts, stop automatic retries and flag the
             row for operational review.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ScheduleOutboxMessageRetry
    @MessageId UNIQUEIDENTIFIER,
    @LockId UNIQUEIDENTIFIER,
    @NextAttemptAtUtc DATETIME2(7),
    @LastError NVARCHAR(2000)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.OutboxMessages
    SET
        Attempts = Attempts + 1,
        NextAttemptAtUtc =
            CASE WHEN Attempts + 1 >= 10 THEN NULL ELSE @NextAttemptAtUtc END,
        FailedAtUtc =
            CASE WHEN Attempts + 1 >= 10 THEN SYSUTCDATETIME() ELSE NULL END,
        LastError = LEFT(@LastError, 2000),
        LockId = NULL,
        LockedUntilUtc = NULL
    WHERE Id = @MessageId
      AND LockId = @LockId
      AND ProcessedAtUtc IS NULL
      AND FailedAtUtc IS NULL;

    IF @@ROWCOUNT = 0
        THROW 51111, 'The Outbox message retry was not scheduled because the lease is no longer owned by this publisher.', 1;
END;
GO
