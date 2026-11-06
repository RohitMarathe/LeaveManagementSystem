using System.Data;
using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Infrastructure.Persistence.Repositories;

/// <summary>
/// Executes the database-owned atomic leave submission operation.
/// The stored procedure inserts both LeaveRequests and OutboxMessages.
/// </summary>
public sealed class LeaveRequestSubmissionRepository
    : ILeaveRequestSubmissionRepository
{
    private readonly LeaveManagementDbContext _context;

    public LeaveRequestSubmissionRepository(LeaveManagementDbContext context)
    {
        _context = context;
    }

    public async Task<int> SubmitWithOutboxAsync(
        int employeeId,
        string leaveType,
        DateOnly startDate,
        DateOnly endDate,
        string reason,
        Guid eventId,
        string correlationId,
        DateTime submittedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_SubmitLeaveRequestWithOutbox";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "@EmployeeId", DbType.Int32, employeeId);
            AddParameter(command, "@LeaveType", DbType.String, leaveType, 50);
            AddParameter(command, "@StartDate", DbType.Date, startDate.ToDateTime(TimeOnly.MinValue));
            AddParameter(command, "@EndDate", DbType.Date, endDate.ToDateTime(TimeOnly.MinValue));
            AddParameter(command, "@Reason", DbType.String, reason, 1000);
            AddParameter(command, "@EventId", DbType.Guid, eventId);
            AddParameter(command, "@CorrelationId", DbType.String, correlationId, 100);
            AddParameter(command, "@SubmittedAtUtc", DbType.DateTime2, submittedAtUtc);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result is null || result is DBNull)
            {
                throw new InvalidOperationException(
                    "The leave submission procedure did not return a LeaveRequest ID.");
            }

            return Convert.ToInt32(result);
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static void AddParameter(
        System.Data.Common.DbCommand command,
        string name,
        DbType type,
        object value,
        int? size = null)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Value = value;

        if (size.HasValue)
        {
            parameter.Size = size.Value;
        }

        command.Parameters.Add(parameter);
    }
}
