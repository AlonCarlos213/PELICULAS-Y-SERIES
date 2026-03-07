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
            Console.WriteLine($"Received Google login request");
            
            if (string.IsNullOrEmpty(request.IdToken))
            {
                Console.WriteLine("IdToken is null or empty");
                return BadRequest("IdToken is required");
            }

            Console.WriteLine($"IdToken length: {request.IdToken.Length}");
            Console.WriteLine($"IdToken preview: {request.IdToken.Substring(0, Math.Min(100, request.IdToken.Length))}...");

            var jwtToken = await _googleAuthService.AuthenticateWithGoogleAsync(request.IdToken);

            Console.WriteLine("Google authentication successful");
            return Ok(new { Token = jwtToken });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Google login failed: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            _logger.LogError(ex, "Google login failed");
            return Unauthorized($"Invalid Google token: {ex.Message}");
        }
    }
}

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}
