extends "chunk_builder.gd"

const blocks = preload("res://scripts/engine/blocks.gd")

@export var height: int = 20
@export var topBlock: blocks.DefaultBlock
@export var middleBlock: blocks.DefaultBlock
@export var bottomBlock: blocks.DefaultBlock

func generate_chunk (chunk: Chunk) -> void:
	chunk.fill_layer (0, bottomBlock)
	for i in range(height):
		chunk.fill_layer(i + 1, middleBlock)
	chunk.fill_layer (height + 1, topBlock)
	chunk.update_mesh()
