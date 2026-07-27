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

        public ICollection<Document> Documents { get; set; }
        = new List<Document>();

        public bool IsActive { get; set; } = true;
    }
}
