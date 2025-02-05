extends Node3D

const Chunk = preload("res://scripts/engine/chunk.gd")

@export var chunk_size: Vector2 = Vector2(16, 16)
@export var chunk_scene: PackedScene
@export var chunk_builder: ChunkBuilder

var chunk_threads: Array[Thread]

func build_chunk (position: Vector2i):
	var chunk: Node3D = chunk_scene.instantiate()
	add_child(chunk)
	chunk.position = Vector3(position.x * chunk_size.x, 0, position.y * chunk_size.y)
	var chunk_class = chunk as Chunk
	chunk_class.chunk_pos = position
	
	var thread = Thread.new()
	chunk_threads.append(thread)
	thread.start(chunk_builder.generate_chunk.bind(chunk_class))

#func _ready() -> void:
	#for x in range(-4, 4):
	#	for z in range(-4, 4):
	#		build_chunk(Vector2i(x, z))
