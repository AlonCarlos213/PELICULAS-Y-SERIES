using Microsoft.AspNetCore.Mvc;
using MyStream.Api.Services;

namespace MyStream.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;

    public AuthController(IGoogleAuthService googleAuthService)
    {
        _googleAuthService = googleAuthService;
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

            Console.WriteLine($"IdToken length: {request.IdToken.Length}");
            Console.WriteLine($"IdToken preview: {request.IdToken.Substring(0, Math.Min(100, request.IdToken.Length))}...");
            
            var jwtToken = await _googleAuthService.AuthenticateGoogleToken(request.IdToken);
            
            Console.WriteLine("Google authentication successful");
            
            return Ok(new { Token = jwtToken });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Google authentication error: {ex.Message}");
            return StatusCode(500, "Internal server error during authentication");
        }
    }
}

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}
