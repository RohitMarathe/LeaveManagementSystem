/*
    Object: dbo.OutboxMessages
    Purpose: Durable storage for integration events awaiting publication.

    Apply this script before the Outbox stored procedures.
    Safe to re-run: missing publisher columns are added to an existing table.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OutboxMessages
    (
        Id               UNIQUEIDENTIFIER NOT NULL,
        EventType        NVARCHAR(200)    NOT NULL,
        Payload          NVARCHAR(MAX)    NOT NULL,
        OccurredAtUtc    DATETIME2(7)     NOT NULL,
        CreatedAtUtc     DATETIME2(7)     NOT NULL
            CONSTRAINT DF_OutboxMessages_CreatedAtUtc
            DEFAULT SYSUTCDATETIME(),
        ProcessedAtUtc   DATETIME2(7)     NULL,
        Attempts         INT              NOT NULL
            CONSTRAINT DF_OutboxMessages_Attempts DEFAULT (0),
        NextAttemptAtUtc DATETIME2(7)     NULL,
        LastError        NVARCHAR(2000)   NULL,
        LockId           UNIQUEIDENTIFIER NULL,
        LockedUntilUtc   DATETIME2(7)     NULL,
        FailedAtUtc      DATETIME2(7)     NULL,

        CONSTRAINT PK_OutboxMessages PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_OutboxMessages_Attempts_NonNegative
            CHECK (Attempts >= 0)
    );

    CREATE NONCLUSTERED INDEX IX_OutboxMessages_PublishingStatus
        ON dbo.OutboxMessages
        (
            ProcessedAtUtc,
            FailedAtUtc,
            NextAttemptAtUtc,
            LockedUntilUtc,
            CreatedAtUtc
        );
END;
GO

/* Upgrade databases where the initial Outbox table script was already run. */
IF COL_LENGTH(N'dbo.OutboxMessages', N'LockId') IS NULL
    ALTER TABLE dbo.OutboxMessages ADD LockId UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH(N'dbo.OutboxMessages', N'LockedUntilUtc') IS NULL
    ALTER TABLE dbo.OutboxMessages ADD LockedUntilUtc DATETIME2(7) NULL;
GO

IF COL_LENGTH(N'dbo.OutboxMessages', N'FailedAtUtc') IS NULL
    ALTER TABLE dbo.OutboxMessages ADD FailedAtUtc DATETIME2(7) NULL;
GO

/* Rebuild the scan index to include claim and terminal-failure state. */
IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.OutboxMessages')
      AND name = N'IX_OutboxMessages_PublishingStatus'
)
    DROP INDEX IX_OutboxMessages_PublishingStatus ON dbo.OutboxMessages;
GO

CREATE NONCLUSTERED INDEX IX_OutboxMessages_PublishingStatus
    ON dbo.OutboxMessages
    (
        ProcessedAtUtc,
        FailedAtUtc,
        NextAttemptAtUtc,
        LockedUntilUtc,
        CreatedAtUtc
    );
GO
