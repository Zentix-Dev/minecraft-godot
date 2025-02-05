extends MeshInstance3D

const blocks_script = preload("res://scripts/engine/blocks.gd")
const mesh_utils = preload("res://scripts/engine/mesh_utils.gd")

const size := Vector3i(16, 256, 16)

var blocks: PackedInt32Array = []
var chunk_pos: Vector2i

var vertices: PackedVector3Array
var triangles: PackedInt32Array
var normals: PackedVector3Array
var uvs: PackedVector2Array

func _init() -> void:
	blocks.resize(size.x * size.y * size.z)
	#fill_layer(0, blocks_script.DefaultBlock.Grass)
	#set_block(Vector3i(10, 1, 10), blocks_script.DefaultBlock.Dirt)

#func _ready() -> void:
#	update_mesh()

func fill_layer (layer: int, block: int):
	var layer_size: int = size.x * size.z
	for i in range(layer_size):
		blocks[i + (layer * layer_size)] = block

func get_block(pos: Vector3i) -> int:
	if pos.x >= size.x or pos.x < 0:
		return 0
	if pos.y >= size.y or pos.y < 0:
		return 0
	if pos.z >= size.z or pos.z < 0:
		return 0
	return blocks[pos.y * (size.x * size.z) + pos.x * size.z + pos.z]

func set_block(pos: Vector3i, block: int) -> void:
	pos.y = clampi(pos.y, 0, size.y - 1)
	pos.x = clampi(pos.x, 0, size.x - 1)
	pos.z = clampi(pos.z, 0, size.z - 1)
	blocks[pos.y * (size.x * size.z) + pos.x * size.z + pos.z] = block

func position_at_index (index: int) -> Vector3i:
	var y: int = index / (size.x * size.z)
	var x: int = (index % (size.x * size.z)) / size.z
	var z: int = index % size.z
	return Vector3i(x, y, z)
	
func add_face (direction: mesh_utils.FaceDirection, position: Vector3i, block: int):
	var texture_index = blocks_script.get_texture_index(block, direction)
	mesh_utils.create_face(direction, position, texture_index, vertices, triangles, normals, uvs)

func update_cube (pos: Vector3i):
	var block: int = get_block(pos)
	if not blocks_script.is_solid(block):
		return
	
	for i in range(mesh_utils.FaceDirection.size()):
		var direction = mesh_utils.FaceDirection.values()[i]
		var vector = mesh_utils.get_direction_vector(direction)
		if not blocks_script.is_solid(get_block(pos + vector)):
			add_face(direction, pos, block)
	
	pass

func create_mesh ():
	if mesh != null:
		mesh.free()
	mesh = mesh_utils.create_array_mesh(vertices, triangles, uvs, normals)

func update_mesh ():
	vertices = []
	normals = []
	triangles = []
	uvs = []
	
	var start_time = Time.get_ticks_msec()
	for y in range(size.y):
		var start_time_a = Time.get_ticks_msec()
		for x in range(size.x):
			for z in range(size.z):
				update_cube(Vector3i(x, y, z))
		print("Generated Layer " + str(y) + ", took " + str(Time.get_ticks_msec() - start_time_a))
	print("Generated Mesh, took " + str(Time.get_ticks_msec() - start_time))
	call_deferred("create_mesh")
