namespace MyStream.Api.DTOs;

public class LibraryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLibraryDto
{
    public string Name { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;
}
