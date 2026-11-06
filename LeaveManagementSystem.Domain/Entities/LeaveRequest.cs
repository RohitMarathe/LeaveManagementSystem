using LeaveManagementSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.Domain.Entities
{
    public class LeaveRequest
    {
        // Unique ID for the leave request
        public int Id { get; set; }

        // Employee who submitted the request
        public int EmployeeId { get; set; }

        // First day of leave
        public DateOnly StartDate { get; set; }

        // Last day of leave
        public DateOnly EndDate { get; set; }

        // Type of leave being requested (e.g., Vacation, Sick, Personal)
        public required string LeaveType { get; set; }

        // Why the employee is requesting leave
        public required string Reason { get; set; }

        // Pending, Approved, Rejected or Cancelled
        public required LeaveRequestStatus Status { get; set; }
        = LeaveRequestStatus.Pending;

        // When the request was submitted
        public DateTime SubmittedAtUtc { get; set; }

        // Navigation property to the employee who submitted the request
        public Employee Employee { get; set; } = default!;
    }
}
