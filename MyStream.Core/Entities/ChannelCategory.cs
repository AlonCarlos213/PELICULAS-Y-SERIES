using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStream.Core.Entities;

public class ChannelCategory
{
    public required Guid LiveChannelId { get; set; }
    public required int CategoryId { get; set; }
    
    [ForeignKey("LiveChannelId")]
    public LiveChannel LiveChannel { get; set; } = null!;
    
    [ForeignKey("CategoryId")]
    public Category Category { get; set; } = null!;
}
