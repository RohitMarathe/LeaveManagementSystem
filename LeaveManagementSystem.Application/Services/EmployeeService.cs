using AutoMapper;
using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Application.Interfaces.Services;
using LeaveManagementSystem.Domain.Common;
using LeaveManagementSystem.Domain.Dtos.Employee;
using System.Net;

namespace LeaveManagementSystem.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EmployeeService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<EmployeeResponseDto>>> GetAllEmployeesAsync(
        CancellationToken cancellationToken = default)
    {
        var employees = await _unitOfWork.Employees.GetAllAsync(cancellationToken);

        var response = new ApiResponse<List<EmployeeResponseDto>>
        {
            IsSuccess = true,
            StatusCode = (int)HttpStatusCode.OK,
            Message = "Employees retrieved successfully.",
            Data = _mapper.Map<List<EmployeeResponseDto>>(employees)
        };

        return response;
    }

    public async Task<ApiResponse<EmployeeResponseDto>> GetEmployeeByIdAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        var employee = await _unitOfWork
            .Employees
            .GetByIdAsync(employeeId, cancellationToken);

        if (employee == null)
        {
            return ApiResponse<EmployeeResponseDto>.Failure(message: $"Employee with ID {employeeId} not found.",
                statusCode: HttpStatusCode.NotFound);
        }

        return ApiResponse<EmployeeResponseDto>.Success(
            data: _mapper.Map<EmployeeResponseDto>(employee),
            message: "Employee retrieved successfully.",
            statusCode: HttpStatusCode.OK);
    }
}