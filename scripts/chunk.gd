class_name Chunk extends StaticBody3D

signal generated(chunk: Chunk)

# Single block vertices
const VERTICES = [
        Vector3i(0, 0, 0),
        Vector3i(1, 0, 0),
        Vector3i(0, 1, 0),
        Vector3i(1, 1, 0),
        Vector3i(0, 0, 1),
        Vector3i(1, 0, 1),
        Vector3i(0, 1, 1),
        Vector3i(1, 1, 1),
]

# Faces vertices painting order
const TOP_FACE = [2, 3, 7, 6]
const BOTTOM_FACE = [0, 4, 5, 1]
const LEFT_FACE = [6, 4, 0, 2]
const RIGHT_FACE = [3, 1, 5, 7]
const FRONT_FACE = [7, 5, 4, 6]
const BACK_FACE = [2, 0, 1, 3]

const DIMENSIONS = Vector3i(8, 64, 8)

@export var chunk_position: Vector2i

var collision_shape_3d = CollisionShape3D.new()
var mesh_instance_3d = MeshInstance3D.new()
var blocks: Array
var chunk_generator = FlatChunkGenerator.new()
var _thread: Thread

func start_generation() -> void:
        _thread = Thread.new()
        _thread.start(self, "_generate_thread")

func _generate_thread(userdata) -> void:
        var local_blocks = []
        local_blocks.resize(DIMENSIONS.x)
        for x in DIMENSIONS.x:
                local_blocks[x] = []
                local_blocks[x].resize(DIMENSIONS.y)
                for y in DIMENSIONS.y:
                        local_blocks[x][y] = []
                        local_blocks[x][y].resize(DIMENSIONS.z)

        for x in range(DIMENSIONS.x):
                for y in range(DIMENSIONS.y):
                        for z in range(DIMENSIONS.z):
                                local_blocks[x][y][z] = chunk_generator.calc_block(Vector3i(x, y, z))

        var st = SurfaceTool.new()
        st.begin(Mesh.PRIMITIVE_TRIANGLES)
        for x in range(DIMENSIONS.x):
                for y in range(DIMENSIONS.y):
                        for z in range(DIMENSIONS.z):
                                if local_blocks[x][y][z] != null:
                                        _create_block(st, Vector3i(chunk_position.x * DIMENSIONS.x + x, y, chunk_position.y * DIMENSIONS.z + z))
                                        st.set_material(local_blocks[x][y][z].material)
        var mesh = st.commit()
        var shape = mesh.create_trimesh_shape()
        call_deferred("_generation_completed", local_blocks, mesh, shape)

func _generation_completed(local_blocks: Array, mesh: Mesh, shape: Shape3D) -> void:
        blocks = local_blocks
        add_child(collision_shape_3d)
        add_child(mesh_instance_3d)
        mesh_instance_3d.mesh = mesh
        collision_shape_3d.shape = shape
        if _thread:
                _thread.wait_to_finish()
        generated.emit(self)

func generate() -> void:

        for x in range(DIMENSIONS.x):
                for y in range(DIMENSIONS.y):
                        for z in range(DIMENSIONS.z):
                                blocks[x][y][z] = chunk_generator.calc_block(Vector3i(x, y, z))

func update() -> void:
        var surface_tool = SurfaceTool.new()
        surface_tool.begin(Mesh.PRIMITIVE_TRIANGLES)
        for x in range(DIMENSIONS.x):
                for y in range(DIMENSIONS.y):
                        for z in range(DIMENSIONS.z):
                                if blocks[x][y][z] != null:
                                        _create_block(surface_tool, Vector3i(chunk_position.x * DIMENSIONS.x + x, y, chunk_position.y * DIMENSIONS.z + z))
                                        surface_tool.set_material(blocks[x][y][z].material)
        var mesh = surface_tool.commit()
        mesh_instance_3d.mesh = mesh
        collision_shape_3d.shape = mesh.create_trimesh_shape()

func _create_block(surface_tool: SurfaceTool, block_position: Vector3i) -> void:
        _create_face(surface_tool, TOP_FACE, block_position)
        _create_face(surface_tool, BOTTOM_FACE, block_position)
        _create_face(surface_tool, LEFT_FACE, block_position)
        _create_face(surface_tool, RIGHT_FACE, block_position)
        _create_face(surface_tool, FRONT_FACE, block_position)
        _create_face(surface_tool, BACK_FACE, block_position)

func _create_face(surface_tool: SurfaceTool, face: Array, block_position: Vector3i) -> void:
        var a: Vector3 = VERTICES[face[0]] + block_position
        var b: Vector3 = VERTICES[face[1]] + block_position
        var c: Vector3 = VERTICES[face[2]] + block_position
        var d: Vector3 = VERTICES[face[3]] + block_position

        var uv_a = Vector2(0, 0)
        var uv_b = Vector2(0, 1.0)
        var uv_c = Vector2(1.0, 1.0)
        var uv_d = Vector2(1.0, 0)

        # Calculate normals
        var side_a = b - a
        var side_b = a - c
        var normal = side_a.cross(side_b)

        surface_tool.add_triangle_fan([a, b, c], [uv_a, uv_b, uv_c], [], [], [normal])
        surface_tool.add_triangle_fan([a, c, d], [uv_a, uv_c, uv_d], [], [], [normal])

func is_block(block_position: Vector3i) -> bool:
        if blocks[block_position.x][block_position.y][block_position.z]:
                return true
        return false
