using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyStream.Core.Entities;
using MyStream.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MyStream.Api.Services;

public interface IGoogleAuthService
{
    Task<string?> AuthenticateGoogleToken(string idToken);
}

public class GoogleAuthService : IGoogleAuthService
{
    private readonly MyStreamDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(MyStreamDbContext context, IConfiguration configuration, ILogger<GoogleAuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> AuthenticateGoogleToken(string idToken)
    {
        try
        {
            _logger.LogInformation($"Starting Google token validation for token: {idToken.Substring(0, Math.Min(50, idToken.Length))}...");

            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { _configuration["Google:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            
            _logger.LogInformation($"Token validated successfully for email: {payload.Email}");
            
            if (_context.Database.IsRelational())
            {
                _logger.LogInformation("Database is relational, proceeding with user operations");
            }
            else
            {
                _logger.LogWarning("Database is not relational, skipping user operations");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                // Determinar rol basado en el correo
                var isAdmin = payload.Email.ToLower() == "carlosalonzomamaniccollque@gmail.com";
                
                // Create new user
                user = new User
                {
                    Email = payload.Email,
                    GoogleId = payload.Subject,
                    DisplayName = $"{payload.GivenName} {payload.FamilyName}".Trim(),
                    ProfilePicture = payload.Picture,
                    Role = isAdmin ? UserRole.Admin : UserRole.User
                };

                _context.Users.Add(user);
                
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Created new user: {user.Email} with role: {user.Role}");
                    Console.WriteLine($"Created new user in database: {user.Email} with role: {user.Role}");
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, $"Database error creating user: {dbEx.InnerException?.Message ?? dbEx.Message}");
                    Console.WriteLine($"Database error creating user: {dbEx.InnerException?.Message ?? dbEx.Message}");
                    throw;
                }
            }
            else
            {
                _logger.LogInformation($"Existing user found: {user.Email} with role: {user.Role}");
            }

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.DisplayName),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation($"JWT token generated successfully for user: {user.Email}");
            Console.WriteLine($"JWT token generated for user: {user.Email}");

            return tokenString;
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogError(ex, $"Invalid Google token: {ex.Message}");
            Console.WriteLine($"Invalid Google token: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error authenticating Google token: {ex.Message}");
            Console.WriteLine($"Error authenticating Google token: {ex.Message}");
            return null;
        }
    }
}
