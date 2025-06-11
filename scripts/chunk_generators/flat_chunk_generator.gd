class_name FlatChunkGenerator extends Resource
var _planks_res: BlockResource = preload("res://assets/blocks/planks.tres")

func calc_block(block_position: Vector3i) -> BlockResource:
	if block_position.y < 1:
		return _planks_res
		#var block = BlockResource.new()
	return null
