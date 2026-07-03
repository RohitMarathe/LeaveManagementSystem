using LeaveManagementSystem.Domain.Common;
using LeaveManagementSystem.Domain.Dtos.Employee;

namespace LeaveManagementSystem.Application.Interfaces.Services;

public interface IEmployeeService
{
    Task<ApiResponse<List<EmployeeResponseDto>>> GetAllEmployeesAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<EmployeeResponseDto>> GetEmployeeByIdAsync(
        int employeeId,
        CancellationToken cancellationToken = default);
}