using Godot;

public partial class ChunkManager : Node3D
{
    [Export]
    public int Radius = 2;

    [Export(PropertyName = "player")]
    public NodePath PlayerPath;

    private CharacterBody3D Player;

    private Vector2I playerChunkPosition;
    private Godot.Collections.Dictionary<Vector2I, Chunk> loadedChunks = new();
    private ChunkLoader chunkLoader = new ChunkLoader();

    public override void _Ready()
    {
        if (PlayerPath != null && PlayerPath != NodePath.Empty)
        {
            Player = GetNode<CharacterBody3D>(PlayerPath);
        }

        playerChunkPosition = GetCurrentPlayerChunkPosition();

        for (int x = -Radius; x <= Radius; x++)
        {
            for (int z = -Radius; z <= Radius; z++)
            {
                Vector2I chunkPosition = playerChunkPosition + new Vector2I(x, z);
                if (loadedChunks.ContainsKey(chunkPosition))
                    continue;
                Chunk chunk = new Chunk();
                chunk.ChunkPosition = chunkPosition;
                chunk.Init();
                loadedChunks[chunkPosition] = chunk;
                AddChild(chunk);
            }
        }

        Chunk currentChunk = (Chunk)loadedChunks[playerChunkPosition];
        Vector3 pcp = (Player.Position / 8).Floor();
        for (int y = (int)pcp.Y; y < 64; y++)
        {
            if (currentChunk.IsBlock(new Vector3I(
                    (int)pcp.X - playerChunkPosition.X * 8,
                    y,
                    (int)pcp.Z - playerChunkPosition.Y * 8
                )) || currentChunk.IsBlock(new Vector3I(
                    (int)pcp.X - playerChunkPosition.X * 8,
                    y + 1,
                    (int)pcp.Z - playerChunkPosition.Y * 8
                )))
                continue;
            Player.Position = new Vector3(Player.Position.X, y + 1, Player.Position.Z);
            break;
        }
    }

    public override void _Process(double delta)
    {
        Vector2I currentPlayerChunkPosition = GetCurrentPlayerChunkPosition();
        if (currentPlayerChunkPosition != playerChunkPosition || chunkLoader.HasLoadedChunks())
        {
            playerChunkPosition = currentPlayerChunkPosition;
            UnloadChunks();
            LoadChunksAtPlayer();
        }
    }

    private Vector2I GetCurrentPlayerChunkPosition()
    {
        if (Player == null)
            return Vector2I.Zero;

        Vector3 pcp = (Player.Position / 8).Floor();
        return new Vector2I((int)pcp.X, (int)pcp.Z);
    }

    private void LoadChunksAtPlayer()
    {
        var newChunkPositions = new Godot.Collections.Array<Vector2I>();
        for (int x = -Radius; x <= Radius; x++)
        {
            for (int z = -Radius; z <= Radius; z++)
            {
                Vector2I chunkPosition = playerChunkPosition + new Vector2I(x, z);
                if (loadedChunks.ContainsKey(chunkPosition))
                    continue;
                newChunkPositions.Add(chunkPosition);
            }
        }
        chunkLoader.PushChunkPositionsToLoad(newChunkPositions);

        var newLoadedChunks = chunkLoader.GetLoadedChunks();
        foreach (Chunk newLoadedChunk in newLoadedChunks)
        {
            loadedChunks[newLoadedChunk.ChunkPosition] = newLoadedChunk;
            AddChild(newLoadedChunk);
        }
    }

    private void UnloadChunks()
    {
        var loadedChunksPositions = new Godot.Collections.Array<Vector2I>(loadedChunks.Keys);
        foreach (Vector2I chunkPosition in loadedChunksPositions)
        {
            if (chunkPosition.X - Radius > playerChunkPosition.X ||
                chunkPosition.X + Radius < playerChunkPosition.X ||
                chunkPosition.Y - Radius > playerChunkPosition.Y ||
                chunkPosition.Y + Radius < playerChunkPosition.Y)
            {
                Chunk chunk = (Chunk)loadedChunks[chunkPosition];
                chunk.QueueFree();
                RemoveChild(chunk);
                loadedChunks.Remove(chunkPosition);
            }
        }
    }
}
