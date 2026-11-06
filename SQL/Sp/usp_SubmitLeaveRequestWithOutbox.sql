/*
    Stored Procedure: dbo.usp_SubmitLeaveRequestWithOutbox
    Purpose:
      1. Validate the employee is eligible to submit a leave request.
      2. Insert the LeaveRequest.
      3. Build the versioned LeaveRequestSubmitted integration event.
      4. Insert the event into dbo.OutboxMessages.
      5. Commit both inserts together, or roll back both.

    Prerequisites:
      - dbo.Employees and dbo.LeaveRequests already exist.
      - dbo.OutboxMessages has been created using
        SQL/Tables/OutboxMessages.sql.

    The API supplies EventId and CorrelationId once. They are persisted
    with the event and must be reused by the publisher on every retry.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_SubmitLeaveRequestWithOutbox
    @EmployeeId       INT,
    @LeaveType        NVARCHAR(MAX),
    @StartDate        DATE,
    @EndDate          DATE,
    @Reason           NVARCHAR(MAX),
    @EventId          UNIQUEIDENTIFIER,
    @CorrelationId    NVARCHAR(100),
    @SubmittedAtUtc   DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @EmployeeId <= 0
        THROW 51000, 'EmployeeId must be a positive value.', 1;

    IF NULLIF(LTRIM(RTRIM(@LeaveType)), N'') IS NULL
        THROW 51000, 'LeaveType is required.', 1;

    IF NULLIF(LTRIM(RTRIM(@Reason)), N'') IS NULL
        THROW 51000, 'Reason is required.', 1;

    IF LEN(LTRIM(RTRIM(@LeaveType))) > 50
        THROW 51000, 'LeaveType cannot exceed 50 characters.', 1;

    IF LEN(LTRIM(RTRIM(@Reason))) > 1000
        THROW 51000, 'Reason cannot exceed 1000 characters.', 1;

    IF @StartDate IS NULL OR @EndDate IS NULL OR @StartDate > @EndDate
        THROW 51000, 'The leave date range is invalid.', 1;

    IF @EventId IS NULL OR @EventId = '00000000-0000-0000-0000-000000000000'
        THROW 51000, 'EventId must be a non-empty GUID.', 1;

    IF NULLIF(LTRIM(RTRIM(@CorrelationId)), N'') IS NULL
        THROW 51000, 'CorrelationId is required.', 1;

    IF @SubmittedAtUtc IS NULL
        THROW 51000, 'SubmittedAtUtc is required.', 1;

    DECLARE
        @EmployeeFirstName NVARCHAR(200),
        @EmployeeLastName  NVARCHAR(200),
        @EmployeeEmail     NVARCHAR(320),
        @IsActive          BIT,
        @ManagerId         INT,
        @LeaveRequestId    INT,
        @EventPayload      NVARCHAR(MAX);

    BEGIN TRY
        BEGIN TRANSACTION;

        /*
          Lock the employee row while checking eligibility and inserting.
          This prevents an employee's active/manager status from changing
          between validation and the leave request insert.
        */
        SELECT
            @EmployeeFirstName = FirstName,
            @EmployeeLastName = LastName,
            @EmployeeEmail = Email,
            @IsActive = IsActive,
            @ManagerId = ManagerId
        FROM dbo.Employees WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @EmployeeId;

        IF @EmployeeFirstName IS NULL
            THROW 51001, 'The employee does not exist.', 1;

        IF @IsActive = 0
            THROW 51002, 'Inactive employees cannot submit leave requests.', 1;

        IF @ManagerId IS NULL
            THROW 51003, 'A manager must be assigned before submitting a leave request.', 1;

        INSERT INTO dbo.LeaveRequests
        (
            EmployeeId,
            StartDate,
            EndDate,
            LeaveType,
            Reason,
            Status,
            SubmittedAtUtc
        )
        VALUES
        (
            @EmployeeId,
            @StartDate,
            @EndDate,
            LTRIM(RTRIM(@LeaveType)),
            LTRIM(RTRIM(@Reason)),
            N'Pending',
            @SubmittedAtUtc
        );

        SET @LeaveRequestId = CONVERT(INT, SCOPE_IDENTITY());

        /*
          Keep this payload aligned with Contracts/LeaveRequestSubmittedEvent.cs.
          Do not include the leave reason because consumers do not require it.
        */
        SELECT @EventPayload =
        (
            SELECT
                @EventId AS eventId,
                1 AS schemaVersion,
                @CorrelationId AS correlationId,
                @LeaveRequestId AS leaveRequestId,
                @EmployeeId AS employeeId,
                LTRIM(RTRIM(
                    CONCAT(@EmployeeFirstName, N' ', @EmployeeLastName)
                )) AS employeeName,
                @EmployeeEmail AS employeeEmail,
                LTRIM(RTRIM(@LeaveType)) AS leaveType,
                CONVERT(CHAR(10), @StartDate, 23) AS startDate,
                CONVERT(CHAR(10), @EndDate, 23) AS endDate,
                @SubmittedAtUtc AS submittedAtUtc
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        INSERT INTO dbo.OutboxMessages
        (
            Id,
            EventType,
            Payload,
            OccurredAtUtc,
            CreatedAtUtc,
            ProcessedAtUtc,
            Attempts,
            NextAttemptAtUtc,
            LastError
        )
        VALUES
        (
            @EventId,
            N'LeaveRequestSubmitted',
            @EventPayload,
            @SubmittedAtUtc,
            SYSUTCDATETIME(),
            NULL,
            0,
            NULL,
            NULL
        );

        COMMIT TRANSACTION;

        SELECT @LeaveRequestId AS LeaveRequestId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO
