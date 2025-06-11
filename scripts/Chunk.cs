using Godot;

[GlobalClass]
public partial class Chunk : StaticBody3D
{
    // Single block vertices
    public static readonly Vector3I[] VERTICES = new Vector3I[]
    {
        new Vector3I(0,0,0),
        new Vector3I(1,0,0),
        new Vector3I(0,1,0),
        new Vector3I(1,1,0),
        new Vector3I(0,0,1),
        new Vector3I(1,0,1),
        new Vector3I(0,1,1),
        new Vector3I(1,1,1)
    };

    // Faces vertices painting order
    public static readonly int[] TOP_FACE = {2,3,7,6};
    public static readonly int[] BOTTOM_FACE = {0,4,5,1};
    public static readonly int[] LEFT_FACE = {6,4,0,2};
    public static readonly int[] RIGHT_FACE = {3,1,5,7};
    public static readonly int[] FRONT_FACE = {7,5,4,6};
    public static readonly int[] BACK_FACE = {2,0,1,3};

    public static readonly Vector3I DIMENSIONS = new Vector3I(8,64,8);

    [Export]
    public Vector2I ChunkPosition { get; set; }

    private CollisionShape3D collisionShape3D = new CollisionShape3D();
    private MeshInstance3D meshInstance3D = new MeshInstance3D();
    private SurfaceTool surfaceTool = new SurfaceTool();
    private BlockResource[,,] blocks;
    private FlatChunkGenerator chunkGenerator = new FlatChunkGenerator();

    public override void _Ready()
    {
    }

    public void Init()
    {
        blocks = new BlockResource[DIMENSIONS.X, DIMENSIONS.Y, DIMENSIONS.Z];

        AddChild(collisionShape3D);
        AddChild(meshInstance3D);

        Generate();
        UpdateChunk();
    }

    public void Generate()
    {
        for (int x = 0; x < DIMENSIONS.X; x++)
        {
            for (int y = 0; y < DIMENSIONS.Y; y++)
            {
                for (int z = 0; z < DIMENSIONS.Z; z++)
                {
                    blocks[x,y,z] = chunkGenerator.CalcBlock(new Vector3I(x,y,z));
                }
            }
        }
    }

    public void UpdateChunk()
    {
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        for (int x = 0; x < DIMENSIONS.X; x++)
        {
            for (int y = 0; y < DIMENSIONS.Y; y++)
            {
                for (int z = 0; z < DIMENSIONS.Z; z++)
                {
                    if (blocks[x,y,z] != null)
                    {
                        CreateBlock(new Vector3I(ChunkPosition.X * DIMENSIONS.X + x, y, ChunkPosition.Y * DIMENSIONS.Z + z));
                        surfaceTool.SetMaterial(blocks[x,y,z].Material);
                    }
                }
            }
        }
        var mesh = surfaceTool.Commit();
        meshInstance3D.Mesh = mesh;
        collisionShape3D.Shape = mesh.CreateTrimeshShape();
    }

    private void CreateBlock(Vector3I blockPosition)
    {
        CreateFace(TOP_FACE, blockPosition);
        CreateFace(BOTTOM_FACE, blockPosition);
        CreateFace(LEFT_FACE, blockPosition);
        CreateFace(RIGHT_FACE, blockPosition);
        CreateFace(FRONT_FACE, blockPosition);
        CreateFace(BACK_FACE, blockPosition);
    }

    private void CreateFace(int[] face, Vector3I blockPosition)
    {
        Vector3 a = VERTICES[face[0]] + (Vector3)blockPosition;
        Vector3 b = VERTICES[face[1]] + (Vector3)blockPosition;
        Vector3 c = VERTICES[face[2]] + (Vector3)blockPosition;
        Vector3 d = VERTICES[face[3]] + (Vector3)blockPosition;

        Vector2 uvA = new Vector2(0,0);
        Vector2 uvB = new Vector2(0,1);
        Vector2 uvC = new Vector2(1,1);
        Vector2 uvD = new Vector2(1,0);

        Vector3 sideA = b - a;
        Vector3 sideB = a - c;
        Vector3 normal = sideA.Cross(sideB);

        surfaceTool.AddTriangleFan(
            new Vector3[] { a, b, c },
            new Vector2[] { uvA, uvB, uvC },
            null,
            null,
            new Vector3[] { normal }
        );
        surfaceTool.AddTriangleFan(
            new Vector3[] { a, c, d },
            new Vector2[] { uvA, uvC, uvD },
            null,
            null,
            new Vector3[] { normal }
        );
    }

    public bool IsBlock(Vector3I blockPosition)
    {
        return blocks[blockPosition.X, blockPosition.Y, blockPosition.Z] != null;
    }
}
