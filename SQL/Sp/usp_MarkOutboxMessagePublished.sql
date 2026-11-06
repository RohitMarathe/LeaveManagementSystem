/*
    Stored Procedure: dbo.usp_MarkOutboxMessagePublished
    Purpose: Mark a successfully sent event as published, only if this
             publisher still owns its lease.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_MarkOutboxMessagePublished
    @MessageId UNIQUEIDENTIFIER,
    @LockId UNIQUEIDENTIFIER,
    @ProcessedAtUtc DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.OutboxMessages
    SET
        ProcessedAtUtc = @ProcessedAtUtc,
        LockId = NULL,
        LockedUntilUtc = NULL,
        LastError = NULL,
        NextAttemptAtUtc = NULL
    WHERE Id = @MessageId
      AND LockId = @LockId
      AND ProcessedAtUtc IS NULL
      AND FailedAtUtc IS NULL;

    IF @@ROWCOUNT = 0
        THROW 51110, 'The Outbox message was not marked as published because the lease is no longer owned by this publisher.', 1;
END;
GO
