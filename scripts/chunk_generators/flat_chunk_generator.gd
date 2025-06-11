class_name FlatChunkGenerator extends Resource

func calc_block(block_position: Vector3i) -> BlockResource:
	if block_position.y < 1:
		return load("res://assets/blocks/planks.tres")
		#var block = BlockResource.new()
	return null
