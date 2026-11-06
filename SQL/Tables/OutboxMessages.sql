/*
    Object: dbo.OutboxMessages
    Purpose: Durable storage for integration events awaiting publication.

    Apply this script before deploying dbo.usp_SubmitLeaveRequestWithOutbox.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OutboxMessages
    (
        Id              UNIQUEIDENTIFIER NOT NULL,
        EventType       NVARCHAR(200)    NOT NULL,
        Payload         NVARCHAR(MAX)    NOT NULL,
        OccurredAtUtc   DATETIME2(7)     NOT NULL,
        CreatedAtUtc    DATETIME2(7)     NOT NULL
            CONSTRAINT DF_OutboxMessages_CreatedAtUtc
            DEFAULT SYSUTCDATETIME(),
        ProcessedAtUtc  DATETIME2(7)     NULL,
        Attempts        INT              NOT NULL
            CONSTRAINT DF_OutboxMessages_Attempts DEFAULT (0),
        NextAttemptAtUtc DATETIME2(7)    NULL,
        LastError       NVARCHAR(2000)   NULL,

        CONSTRAINT PK_OutboxMessages PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_OutboxMessages_Attempts_NonNegative
            CHECK (Attempts >= 0)
    );

    CREATE NONCLUSTERED INDEX IX_OutboxMessages_PublishingStatus
        ON dbo.OutboxMessages
        (
            ProcessedAtUtc,
            NextAttemptAtUtc,
            CreatedAtUtc
        );
END;
GO
