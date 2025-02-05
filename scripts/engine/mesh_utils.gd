const texture_width: float = 256
const texture_height: float = 256
const rows: int = 16
const columns: int = 16

enum FaceDirection {
	NORTH, SOUTH,
	EAST, WEST,
	UP, DOWN
}

static func get_direction_vector (direction: FaceDirection) -> Vector3i:
	match direction:
		FaceDirection.NORTH:
			return Vector3i(0, 0, 1)
		FaceDirection.SOUTH:
			return Vector3i(0, 0, -1)
		FaceDirection.EAST:
			return Vector3i(1, 0, 0)
		FaceDirection.WEST:
			return Vector3i(-1, 0, 0)
		FaceDirection.UP:
			return Vector3i(0, 1, 0)
		FaceDirection.DOWN:
			return Vector3i(0, -1, 0)
	return Vector3i.ZERO

static func create_array_mesh (vertices: PackedVector3Array, indices: PackedInt32Array, uvs: PackedVector2Array = [], normals: PackedVector3Array = []) -> ArrayMesh:
	var arrays: Array[Variant] = [];
	arrays.resize(ArrayMesh.ARRAY_MAX as int);
	arrays[ArrayMesh.ARRAY_VERTEX as int] = vertices;
	arrays[ArrayMesh.ARRAY_INDEX as int] = indices;
	arrays[ArrayMesh.ARRAY_TEX_UV as int] = uvs;
	uvs.resize(vertices.size())
	arrays[ArrayMesh.ARRAY_NORMAL as int] = normals;
	normals.resize(vertices.size())
	
	var array_mesh: ArrayMesh = ArrayMesh.new();
	array_mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays);
	return array_mesh;

static func texture_index_to_uvs(texture_index: int) -> PackedVector2Array:
	var width: float = (texture_width / columns) / texture_width
	var height: float = (texture_height / rows) / texture_height
	var x: float = (texture_index % columns) * width
	var y: float = ((texture_index / columns) as int) * height
	
	return [
		Vector2(x + width, y),
		Vector2(x + width, y + width),
		Vector2(x, y + width),
		Vector2(x, y)
	]

static func create_face (direction: FaceDirection, offset: Vector3, texture_index: int, vertices: PackedVector3Array, indices: PackedInt32Array, normals: PackedVector3Array, uvs: PackedVector2Array) -> void:
	var array_offset: int = vertices.size()
	
	match direction:
		FaceDirection.NORTH, FaceDirection.SOUTH:
			var dir: int = 1 if direction == FaceDirection.NORTH else -1
			var verts: PackedVector3Array = [
				Vector3(.5, .5, .5 * dir) + offset,
				Vector3(.5, -.5, .5 * dir) + offset,
				Vector3(-.5, -.5, .5 * dir) + offset,
				Vector3(-.5, .5, .5 * dir) + offset,
			]
			var normal: Vector3 = Vector3(0, 0, dir)
			normals.append_array([normal, normal, normal, normal])
			if dir == -1:
				verts.reverse()
			vertices.append_array(verts);
		
		FaceDirection.EAST, FaceDirection.WEST:
			var dir: int = 1 if direction == FaceDirection.EAST else -1
			var verts: PackedVector3Array = [
				Vector3(.5 * dir, .5, -.5) + offset,
				Vector3(.5 * dir, -.5, -.5) + offset,
				Vector3(.5 * dir, -.5, .5) + offset,
				Vector3(.5 * dir, .5, .5) + offset,
			]
			var normal: Vector3 = Vector3(dir, 0, 0)
			normals.append_array([normal, normal, normal, normal])
			if dir == -1:
				verts.reverse()
			vertices.append_array(verts)
			
		FaceDirection.UP, FaceDirection.DOWN:
			var dir: int = 1 if direction == FaceDirection.UP else -1
			var verts: PackedVector3Array = [
				Vector3(-.5, .5 * dir, .5) + offset,
				Vector3(-.5, .5 * dir, -.5) + offset,
				Vector3(.5, .5 * dir, -.5) + offset,
				Vector3(.5, .5 * dir, .5) + offset,
			]
			var normal: Vector3 = Vector3(0, dir, 0)
			normals.append_array([normal, normal, normal, normal])
			if dir == -1:
				verts.reverse()
			vertices.append_array(verts);
	
	indices.append_array([
		0 + array_offset, 1 + array_offset, 2 + array_offset,
		2 + array_offset, 3 + array_offset, 0 + array_offset
	])
	var uvmappings = texture_index_to_uvs(texture_index)
	uvs.append_array(uvmappings)
