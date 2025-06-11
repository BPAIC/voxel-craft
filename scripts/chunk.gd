class_name Chunk extends StaticBody3D

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
var surface_tool: SurfaceTool = SurfaceTool.new()
var blocks: Array
var chunk_generator = FlatChunkGenerator.new()

func _ready() -> void:
	pass

func init() -> void:
	# Prepare blocks array
	blocks = []
	blocks.resize(DIMENSIONS.x)
	for x in DIMENSIONS.x:
		blocks[x] = []
		blocks[x].resize(DIMENSIONS.y)
		for y in DIMENSIONS.y:
			blocks[x][y] = []
			blocks[x][y].resize(DIMENSIONS.z)
			
	add_child(collision_shape_3d)
	add_child(mesh_instance_3d)
			
	generate()
	update()

func generate() -> void:
	for x in range(DIMENSIONS.x):
		for y in range(DIMENSIONS.y):
			for z in range(DIMENSIONS.z):
				blocks[x][y][z] = chunk_generator.calc_block(Vector3i(x, y, z))
		
func update() -> void:
	surface_tool.begin(Mesh.PRIMITIVE_TRIANGLES)
	for x in range(DIMENSIONS.x):
		for y in range(DIMENSIONS.y):
			for z in range(DIMENSIONS.z):
				if blocks[x][y][z] != null:
					create_block(Vector3i(chunk_position.x * DIMENSIONS.x + x, y, chunk_position.y * DIMENSIONS.z + z))
					surface_tool.set_material(blocks[x][y][z].material)
	var mesh = surface_tool.commit()
	mesh_instance_3d.mesh = mesh
	collision_shape_3d.shape = mesh.create_trimesh_shape()
				
func create_block(block_position: Vector3i) -> void:
	create_face(TOP_FACE, block_position)
	create_face(BOTTOM_FACE, block_position)
	create_face(LEFT_FACE, block_position)
	create_face(RIGHT_FACE, block_position)
	create_face(FRONT_FACE, block_position)
	create_face(BACK_FACE, block_position)
				
func create_face(face: Array, block_position: Vector3i) -> void:
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
