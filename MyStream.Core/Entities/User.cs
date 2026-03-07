namespace MyStream.Core.Entities;

public class User
{
    public int Id { get; set; }
    public required string GoogleId { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public string? ProfilePicture { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int IsActive { get; set; } = 1; // Use integer instead of boolean
}
