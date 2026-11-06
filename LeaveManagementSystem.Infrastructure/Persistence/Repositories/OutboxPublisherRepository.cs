using System.Data;
using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Application.Models.Outbox;
using LeaveManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LeaveManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class OutboxPublisherRepository : IOutboxPublisherRepository
{
    private readonly LeaveManagementDbContext _context;

    public OutboxPublisherRepository(LeaveManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<OutboxMessageToPublish>> ClaimBatchAsync(
        Guid lockId,
        int batchSize,
        int lockDurationSeconds,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<OutboxMessageToPublish>();
        var connection = _context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "dbo.usp_ClaimOutboxMessages";
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

            AddParameter(command, "@LockId", DbType.Guid, lockId);
            AddParameter(command, "@BatchSize", DbType.Int32, batchSize);
            AddParameter(command, "@LockDurationSeconds", DbType.Int32, lockDurationSeconds);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var idOrdinal = reader.GetOrdinal("Id");
            var eventTypeOrdinal = reader.GetOrdinal("EventType");
            var payloadOrdinal = reader.GetOrdinal("Payload");
            var occurredAtOrdinal = reader.GetOrdinal("OccurredAtUtc");
            var attemptsOrdinal = reader.GetOrdinal("Attempts");

            while (await reader.ReadAsync(cancellationToken))
            {
                messages.Add(new OutboxMessageToPublish(
                    reader.GetGuid(idOrdinal),
                    reader.GetString(eventTypeOrdinal),
                    reader.GetString(payloadOrdinal),
                    reader.GetDateTime(occurredAtOrdinal),
                    reader.GetInt32(attemptsOrdinal)));
            }

            return messages;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    public Task MarkPublishedAsync(
        Guid messageId,
        Guid lockId,
        DateTime processedAtUtc,
        CancellationToken cancellationToken = default)
    {
        return ExecuteStatusProcedureAsync(
            "dbo.usp_MarkOutboxMessagePublished",
            command =>
            {
                AddParameter(command, "@MessageId", DbType.Guid, messageId);
                AddParameter(command, "@LockId", DbType.Guid, lockId);
                AddParameter(command, "@ProcessedAtUtc", DbType.DateTime2, processedAtUtc);
            },
            cancellationToken);
    }

    public Task ScheduleRetryAsync(
        Guid messageId,
        Guid lockId,
        DateTime nextAttemptAtUtc,
        string error,
        CancellationToken cancellationToken = default)
    {
        return ExecuteStatusProcedureAsync(
            "dbo.usp_ScheduleOutboxMessageRetry",
            command =>
            {
                AddParameter(command, "@MessageId", DbType.Guid, messageId);
                AddParameter(command, "@LockId", DbType.Guid, lockId);
                AddParameter(command, "@NextAttemptAtUtc", DbType.DateTime2, nextAttemptAtUtc);
                AddParameter(command, "@LastError", DbType.String, error, 2000);
            },
            cancellationToken);
    }

    private async Task ExecuteStatusProcedureAsync(
        string procedureName,
        Action<System.Data.Common.DbCommand> configureCommand,
        CancellationToken cancellationToken)
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
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            configureCommand(command);
            await command.ExecuteNonQueryAsync(cancellationToken);
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
