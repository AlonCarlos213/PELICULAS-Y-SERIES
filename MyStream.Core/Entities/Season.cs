using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStream.Core.Entities;

public class Season
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public required Guid MediaItemId { get; set; }
    public int SeasonNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    [ForeignKey("MediaItemId")]
    public MediaItem MediaItem { get; set; } = null!;
    
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
