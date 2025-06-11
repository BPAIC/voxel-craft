using Godot;

[GlobalClass]
public partial class ChunkLoader : Resource
{
    private Thread thread = new Thread();
    private bool isRunning = true;
    private Semaphore sem = new Semaphore();

    private Godot.Collections.Array<Vector2I> chunkPositionsToLoad = new();
    private Mutex chunkPositionsToLoadMtx = new Mutex();

    private Godot.Collections.Array<Chunk> loadedChunks = new();
    private Mutex loadedChunksMtx = new Mutex();

    private Godot.Collections.Array<Vector2I> chunkPositionsToLoadLocal = new();
    private Godot.Collections.Array<Chunk> loadedChunksLocal = new();

    public override void _Init()
    {
        thread.Start(Callable.From(() => Loop()));
    }

    public override void _ExitTree()
    {
        isRunning = false;
        sem.Post();
        thread.WaitToFinish();
    }

    public void PushChunkPositionsToLoad(Godot.Collections.Array<Vector2I> newChunkPositionsToLoad)
    {
        loadedChunksMtx.Lock();
        chunkPositionsToLoadMtx.Lock();

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

        chunkPositionsToLoadMtx.Unlock();
        loadedChunksMtx.Unlock();
        sem.Post();
    }

    public bool HasLoadedChunks()
    {
        bool result = !loadedChunks.IsEmpty();
        loadedChunksMtx.Lock();
        loadedChunksMtx.Unlock();
        return result;
    }

    public Godot.Collections.Array<Chunk> GetLoadedChunks()
    {
        loadedChunksMtx.Lock();
        var tmp = loadedChunks.Duplicate();
        loadedChunks.Clear();
        loadedChunksMtx.Unlock();
        return tmp;
    }

    private void Loop()
    {
        while (isRunning)
        {
            sem.Wait();

            chunkPositionsToLoadMtx.Lock();
            chunkPositionsToLoadLocal = chunkPositionsToLoad.Duplicate();
            chunkPositionsToLoad.Clear();
            chunkPositionsToLoadMtx.Unlock();

            foreach (Vector2I pos in chunkPositionsToLoadLocal)
            {
                Chunk chunk = new Chunk();
                chunk.ChunkPosition = pos;
                chunk.Init();
                loadedChunksLocal.Add(chunk);
            }

            loadedChunksMtx.Lock();
            loadedChunks.AddRange(loadedChunksLocal);
            loadedChunksMtx.Unlock();
            loadedChunksLocal.Clear();
        }
    }
}
