namespace FlavorQuest.Models;

/// <summary>
/// Represents a food category with its display properties.
/// </summary>
public class Category
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Color Color { get; set; } = Colors.Gray;

    public override string ToString() => Name;
}
