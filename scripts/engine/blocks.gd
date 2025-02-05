const FaceDirection = preload("res://scripts/engine/mesh_utils.gd").FaceDirection

enum DefaultBlock {
	Air, Stone, Dirt, Grass
}

enum BlockSide {
	UP, DOWN, SIDE
}

static func is_solid (block: int) -> bool:
	return block != DefaultBlock.Air

static func get_texture_index (block: int, direction: FaceDirection) -> int:
	var side: BlockSide
	match direction:
		FaceDirection.UP:
			side = BlockSide.UP
		FaceDirection.DOWN:
			side = BlockSide.DOWN
		FaceDirection.NORTH, FaceDirection.SOUTH, FaceDirection.EAST, FaceDirection.WEST:
			side = BlockSide.SIDE
	match block:
		1:
			return 3
		2:
			return 1
		3:
			match side:
				BlockSide.UP:
					return 0
				BlockSide.SIDE:
					return 2
				BlockSide.DOWN:
					return 1
	return -1
