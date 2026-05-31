using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class CatalogItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Category { get; set; } = default!;

    [Required]
    public string ExternalId { get; set; } = default!;

    [Required]
    public string Title { get; set; } = default!;

    public string? ImageUrl { get; set; }
    public string? MetaJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

