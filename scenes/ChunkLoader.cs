using Godot;
using System.Threading;

[GlobalClass]
public partial class ChunkLoader : Resource
{
    private Thread thread;
    private bool isRunning = true;
    private AutoResetEvent sem = new AutoResetEvent(false);

    private Godot.Collections.Array<Vector2I> chunkPositionsToLoad = new();
    private readonly object chunkPositionsToLoadMtx = new object();

    private Godot.Collections.Array<Chunk> loadedChunks = new();
    private readonly object loadedChunksMtx = new object();

    private Godot.Collections.Array<Vector2I> chunkPositionsToLoadLocal = new();
    private Godot.Collections.Array<Chunk> loadedChunksLocal = new();

    public ChunkLoader()
    {
        thread = new Thread(Loop);
        thread.Start();
    }

    ~ChunkLoader()
    {
        isRunning = false;
        sem.Set();
        thread.Join();
    }

    public void PushChunkPositionsToLoad(Godot.Collections.Array<Vector2I> newChunkPositionsToLoad)
    {
        Monitor.Enter(loadedChunksMtx);
        Monitor.Enter(chunkPositionsToLoadMtx);

        foreach (Vector2I pos in chunkPositionsToLoadLocal)
        {
            if (newChunkPositionsToLoad.Contains(pos))
                newChunkPositionsToLoad.Remove(pos);
        }
        foreach (Vector2I pos in chunkPositionsToLoad)
        {
            if (newChunkPositionsToLoad.Contains(pos))
                newChunkPositionsToLoad.Remove(pos);
        }
        foreach (Chunk chunk in loadedChunks)
        {
            if (newChunkPositionsToLoad.Contains(chunk.ChunkPosition))
                newChunkPositionsToLoad.Remove(chunk.ChunkPosition);
        }

        chunkPositionsToLoad.AddRange(newChunkPositionsToLoad);

        Monitor.Exit(chunkPositionsToLoadMtx);
        Monitor.Exit(loadedChunksMtx);
        sem.Set();
    }

    public bool HasLoadedChunks()
    {
        bool result = !loadedChunks.IsEmpty();
        Monitor.Enter(loadedChunksMtx);
        Monitor.Exit(loadedChunksMtx);
        return result;
    }

    public Godot.Collections.Array<Chunk> GetLoadedChunks()
    {
        Monitor.Enter(loadedChunksMtx);
        var tmp = loadedChunks.Duplicate();
        loadedChunks.Clear();
        Monitor.Exit(loadedChunksMtx);
        return tmp;
    }

    private void Loop()
    {
        while (isRunning)
        {
            sem.WaitOne();

            Monitor.Enter(chunkPositionsToLoadMtx);
            chunkPositionsToLoadLocal = chunkPositionsToLoad.Duplicate();
            chunkPositionsToLoad.Clear();
            Monitor.Exit(chunkPositionsToLoadMtx);

            foreach (Vector2I pos in chunkPositionsToLoadLocal)
            {
                Chunk chunk = new Chunk();
                chunk.ChunkPosition = pos;
                chunk.Init();
                loadedChunksLocal.Add(chunk);
            }

            Monitor.Enter(loadedChunksMtx);
            loadedChunks.AddRange(loadedChunksLocal);
            Monitor.Exit(loadedChunksMtx);
            loadedChunksLocal.Clear();
        }
    }
}
