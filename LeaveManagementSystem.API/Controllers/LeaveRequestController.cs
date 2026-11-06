
using LeaveManagementSystem.Application.Interfaces.Services;
using LeaveManagementSystem.Domain.Dtos.LeaveRequest;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LeaveRequestController : BaseController
{
    private readonly ILeaveRequestService _service;
    private readonly ILogger<LeaveRequestController> _logger;

    public LeaveRequestController(
        ILeaveRequestService service,
        ILogger<LeaveRequestController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Submit(
        [FromBody] CreateLeaveRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Received leave request submission for EmployeeId: {EmployeeId}",
            request.EmployeeId);

        var response = await _service.SubmitAsync(
            request,
            cancellationToken);

        if (response.IsSuccess)
        {
            _logger.LogInformation(
                "Leave request submitted. LeaveRequestId: {LeaveRequestId}",
                response.Data?.Id);
        }
        else
        {
            _logger.LogWarning(
                "Leave request submission failed for EmployeeId: {EmployeeId}. Reason: {Message}",
                request.EmployeeId,
                response.Message);
        }

        return ApiResponse(response);
    }
}
