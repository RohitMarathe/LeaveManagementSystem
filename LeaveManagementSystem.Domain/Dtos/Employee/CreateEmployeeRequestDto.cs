namespace LeaveManagementSystem.Domain.Dtos.Employee
{
    public class CreateEmployeeRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public decimal Salary { get; set; }
    }
}
