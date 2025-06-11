using Godot;

[GlobalClass]
public partial class FlatChunkGenerator : Resource
{
    public BlockResource CalcBlock(Vector3I blockPosition)
    {
        if (blockPosition.Y < 1)
        {
            return GD.Load<BlockResource>("res://assets/blocks/planks.tres");
        }
        return null;
    }
}
