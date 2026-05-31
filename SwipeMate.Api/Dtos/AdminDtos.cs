namespace SwipeMate.Api.Dtos;

public sealed class BlockUserDto
{
    public string? Reason { get; set; }
}

public sealed class UpdateCatalogItemStatusDto
{
    public bool IsActive { get; set; }
}
