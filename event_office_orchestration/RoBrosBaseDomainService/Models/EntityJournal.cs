using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoBrosBaseDomainService.Models;

[Table("entity_journals")]
public class EntityJournal
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("entity_id")]
    [MaxLength(255)]
    public string EntityId { get; set; } = string.Empty;

    [Required]
    [Column("entity_type")]
    [MaxLength(255)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [Column("entity")]
    [Column(TypeName = "jsonb")]
    public string Entity { get; set; } = string.Empty;

    [Required]
    [Column("created_by")]
    [MaxLength(255)]
    public string CreatedBy { get; set; } = string.Empty;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("updated_by")]
    [MaxLength(255)]
    public string UpdatedBy { get; set; } = string.Empty;

    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("version")]
    public int Version { get; set; }
}