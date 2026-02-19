using System.ComponentModel.DataAnnotations;

namespace SwipeMate.Api.Models;

public class SessionItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    // "Movie", "Restaurant" и т.н. (или взимаш от Session.Category)
    [Required]
    public string Category { get; set; } = default!;

    // ID от външен източник или твой вътрешен
    [Required]
    public string ExternalId { get; set; } = default!;

    [Required]
    public string Title { get; set; } = default!;

    public string? ImageUrl { get; set; }
    public string? MetaJson { get; set; } // за допълнителни данни (жанр, година...)
}
