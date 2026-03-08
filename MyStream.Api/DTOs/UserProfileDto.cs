namespace MyStream.Api.DTOs;

public class UserProfileDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
}

public class UpdateProfileDto
{
    public string DisplayName { get; set; } = string.Empty;
}
