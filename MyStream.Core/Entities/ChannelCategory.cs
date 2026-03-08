using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStream.Core.Entities;

public class ChannelCategory
{
    public Guid LiveChannelId { get; set; }
    public int CategoryId { get; set; }
    public virtual LiveChannel LiveChannel { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
}
