using Godot;

[GlobalClass]
public partial class BlockResource : Resource
{
    public enum TextureDirection
    {
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back
    }

    [Export]
    public int Id { get; set; }

    [Export]
    public string Name { get; set; } = string.Empty;

    [Export]
    public Material Material { get; set; }
}
