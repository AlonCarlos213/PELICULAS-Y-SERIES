using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStream.Core.Entities;

public class Episode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public required Guid SeasonId { get; set; }
    public int EpisodeNumber { get; set; }
    public string? Title { get; set; }
    public string? LocalPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    [ForeignKey("SeasonId")]
    public Season Season { get; set; } = null!;
}
