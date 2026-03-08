using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MyStream.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminTestController : ControllerBase
{
    [HttpGet("check")]
    [Authorize(Roles = "Admin")]
    public IActionResult CheckAdminAccess()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            Message = "Access granted - Admin role confirmed",
            UserId = userId,
            Email = email,
            Role = role,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("user-check")]
    [Authorize(Roles = "User")]
    public IActionResult CheckUserAccess()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            Message = "Access granted - User role confirmed",
            UserId = userId,
            Email = email,
            Role = role,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("any-authenticated")]
    [Authorize]
    public IActionResult CheckAnyAuthenticated()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            Message = "Access granted - Any authenticated user",
            UserId = userId,
            Email = email,
            Role = role,
            Timestamp = DateTime.UtcNow
        });
    }
}
