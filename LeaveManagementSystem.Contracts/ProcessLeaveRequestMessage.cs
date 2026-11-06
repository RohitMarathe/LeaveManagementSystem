
namespace LeaveManagementSystem.Contracts;

public sealed class ProcessLeaveRequestMessage
{
    public int MessageVersion { get; init; } = 1;

    public int LeaveRequestId { get; init; }
}
