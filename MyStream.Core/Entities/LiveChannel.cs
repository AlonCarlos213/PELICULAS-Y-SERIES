using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStream.Core.Entities;

public class LiveChannel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string StreamUrl { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsOnline { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<ChannelCategory> ChannelCategories { get; set; } = new List<ChannelCategory>();
}
