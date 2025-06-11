extends Node3D

@export var radius: int = 2
@export var player: CharacterBody3D

var player_chunk_position: Vector2i
var loaded_chunks: Dictionary
var chunk_loader = ChunkLoader.new()

func _ready() -> void:
	player_chunk_position = get_current_player_chunk_position()	
	#load_chunks_at_player()
	
	for x in range(-radius, radius + 1):
		for z in range(-radius, radius + 1):
			var chunk_position = player_chunk_position + Vector2i(x, z)
			if loaded_chunks.has(chunk_position):
				continue
			var chunk = Chunk.new()
			chunk.chunk_position = chunk_position
			chunk.init()
			loaded_chunks[chunk_position] = chunk
			add_child(chunk)
	
	# Check that player not stuck in current position
	var current_chunk: Chunk = loaded_chunks[player_chunk_position]
	var pcp = floor(player.position / 8)
	for y in range(pcp.y, 64):
		if current_chunk.is_block(Vector3i(
			pcp.x - player_chunk_position.x * 8,
			y,
			pcp.z - player_chunk_position.y * 8,
		)) or current_chunk.is_block(Vector3i(
			pcp.x - player_chunk_position.x * 8,
			y + 1,
			pcp.z - player_chunk_position.y * 8,
		)):
			continue
		player.position.y = y + 1
		break

func _process(_delta: float) -> void:
	var current_player_chunk_position = get_current_player_chunk_position()
	if current_player_chunk_position != player_chunk_position or chunk_loader.has_loaded_chunks():
		player_chunk_position = current_player_chunk_position
		unload_chunks()
		load_chunks_at_player()

func get_current_player_chunk_position() -> Vector2i:
	# Warning: need to find way how to syncronize size of chunk
	var pcp = floor(player.position / 8)
	return Vector2i(pcp.x, pcp.z) 

func load_chunks_at_player() -> void:
	var new_chunk_positions = []
	for x in range(-radius, radius + 1):
		for z in range(-radius, radius + 1):
			var chunk_position = player_chunk_position + Vector2i(x, z)
			if loaded_chunks.has(chunk_position):
				continue
			new_chunk_positions.append(chunk_position)
			
	chunk_loader.push_chunk_positions_to_load(new_chunk_positions)
	
	# Read loaded chunks by thread
	var new_loaded_chunks = chunk_loader.get_loaded_chunks()
	for new_loaded_chunk in new_loaded_chunks:
		loaded_chunks[new_loaded_chunk.chunk_position] = new_loaded_chunk
		add_child(new_loaded_chunk)

func unload_chunks() -> void:
	var loaded_chunks_positions: Array = loaded_chunks.keys()
	for chunk_position in loaded_chunks_positions:
		if (chunk_position.x - radius > player_chunk_position.x or \
			chunk_position.x + radius < player_chunk_position.x or \
			chunk_position.y - radius > player_chunk_position.y or \
			chunk_position.y + radius < player_chunk_position.y):
			loaded_chunks[chunk_position].queue_free()
			remove_child(loaded_chunks[chunk_position])
			loaded_chunks.erase(chunk_position)
