using Godot;

public partial class CoordsLabel : Label
{
    private CharacterBody3D player;

    public override void _Ready()
    {
        player = GetNode<CharacterBody3D>("/root/World/Player");
        UpdateText();
    }

    public override void _Process(double delta)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        Text = $"Coords: {player.Position}";
    }
}
