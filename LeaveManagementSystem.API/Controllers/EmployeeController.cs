using LeaveManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : BaseController
{
    private readonly IEmployeeService _service;

    public EmployeeController(
        IEmployeeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await _service.GetAllEmployeesAsync(cancellationToken);

        return ApiResponse(response);
    }

    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetById(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var response = await _service.GetEmployeeByIdAsync(
            employeeId,
            cancellationToken);

        return ApiResponse(response);
    }
}