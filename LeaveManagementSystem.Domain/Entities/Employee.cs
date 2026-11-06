namespace LeaveManagementSystem.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public int? ManagerId { get; set; }

        public Employee? Manager { get; set; }

        public ICollection<Employee> DirectReports { get; set; }
            = new List<Employee>();

        public ICollection<LeaveRequest> LeaveRequests { get; set; }
            = new List<LeaveRequest>();

        public ICollection<Document> Documents { get; set; }
        = new List<Document>();

        public bool IsActive { get; set; } = true;
    }
}
