using LeaveManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : BaseController
{
    private readonly IEmployeeService _service;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeService service, ILogger<EmployeeController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Request received to fetch all employees.");

        var response = await _service.GetAllEmployeesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully fetched all employees. Total Employees: {EmployeeCount}",
            response.Data?.Count ?? 0);

        return ApiResponse(response);
    }

    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetById(
        int employeeId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Request received to fetch employee. EmployeeId: {EmployeeId}",
            employeeId);

        var response = await _service.GetEmployeeByIdAsync(
            employeeId,
            cancellationToken);

        if (response.IsSuccess)
        {
            _logger.LogInformation(
                "Employee fetched successfully. EmployeeId: {EmployeeId}",
                employeeId);
        }
        else
        {
            _logger.LogWarning(
                "Employee not found. EmployeeId: {EmployeeId}",
                employeeId);
        }

        return ApiResponse(response);
    }
}