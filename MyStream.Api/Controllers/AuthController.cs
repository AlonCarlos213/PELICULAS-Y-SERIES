using Microsoft.AspNetCore.Mvc;
using MyStream.Api.Services;

namespace MyStream.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IGoogleAuthService googleAuthService,
        ILogger<AuthController> logger)
    {
        _googleAuthService = googleAuthService;
        _logger = logger;
    }

    [HttpPost("login/google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.IdToken))
            {
                return BadRequest("IdToken is required");
            }

            var jwtToken = await _googleAuthService.AuthenticateWithGoogleAsync(request.IdToken);

            return Ok(new { Token = jwtToken });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google login failed");
            return Unauthorized("Invalid Google token");
        }
    }
}

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}
