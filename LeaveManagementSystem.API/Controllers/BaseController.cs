using LeaveManagementSystem.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult ApiResponse<T>(ApiResponse<T> response)
        {
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
