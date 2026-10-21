namespace ARENDATOR.Web.Models;

public class PropertyImage
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = default!;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string? Caption { get; set; }
}
