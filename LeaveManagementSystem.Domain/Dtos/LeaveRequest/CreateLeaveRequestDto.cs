
using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.Domain.Dtos.LeaveRequest;

public sealed class CreateLeaveRequestDto
{
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(50)]
    public string LeaveType { get; set; } = string.Empty;

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}
