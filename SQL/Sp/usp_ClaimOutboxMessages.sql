/*
    Stored Procedure: dbo.usp_ClaimOutboxMessages
    Purpose: Atomically lease a batch of due Outbox messages to one publisher run.

    READPAST skips rows currently locked by another claimant. UPDLOCK keeps
    selected rows reserved until this update transaction completes.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ClaimOutboxMessages
    @LockId UNIQUEIDENTIFIER,
    @BatchSize INT,
    @LockDurationSeconds INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @LockId IS NULL OR @LockId = '00000000-0000-0000-0000-000000000000'
        THROW 51100, 'A non-empty LockId is required.', 1;

    IF @BatchSize <= 0 OR @BatchSize > 500
        THROW 51100, 'BatchSize must be between 1 and 500.', 1;

    IF @LockDurationSeconds < 30 OR @LockDurationSeconds > 3600
        THROW 51100, 'LockDurationSeconds must be between 30 and 3600.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        ;WITH MessagesToClaim AS
        (
            SELECT TOP (@BatchSize) *
            FROM dbo.OutboxMessages WITH (UPDLOCK, READPAST, ROWLOCK, READCOMMITTEDLOCK)
            WHERE ProcessedAtUtc IS NULL
              AND FailedAtUtc IS NULL
              AND (NextAttemptAtUtc IS NULL OR NextAttemptAtUtc <= SYSUTCDATETIME())
              AND (LockedUntilUtc IS NULL OR LockedUntilUtc <= SYSUTCDATETIME())
            ORDER BY CreatedAtUtc, Id
        )
        UPDATE MessagesToClaim
        SET
            LockId = @LockId,
            LockedUntilUtc = DATEADD(SECOND, @LockDurationSeconds, SYSUTCDATETIME())
        OUTPUT
            inserted.Id,
            inserted.EventType,
            inserted.Payload,
            inserted.OccurredAtUtc,
            inserted.Attempts;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
