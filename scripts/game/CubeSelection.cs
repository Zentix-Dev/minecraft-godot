using Godot;

namespace Minecraft.scripts.game;

public partial class CubeSelection : MeshInstance3D
{
	private const float CubeScale = 1.0001f;
	
	public override void _Ready()
	{
		var vertices = new[]
		{
			new Vector3(-0.5f, -0.5f, -0.5f) * CubeScale,  // Vertex 0
			new Vector3(0.5f, -0.5f, -0.5f) * CubeScale,   // Vertex 1
			new Vector3(0.5f, 0.5f, -0.5f) * CubeScale,    // Vertex 2
			new Vector3(-0.5f, 0.5f, -0.5f) * CubeScale,   // Vertex 3
			new Vector3(-0.5f, -0.5f, 0.5f) * CubeScale,   // Vertex 4
			new Vector3(0.5f, -0.5f, 0.5f) * CubeScale,    // Vertex 5
			new Vector3(0.5f, 0.5f, 0.5f) * CubeScale,     // Vertex 6
			new Vector3(-0.5f, 0.5f, 0.5f) * CubeScale     // Vertex 7
		};
		
		int[] indices = {
			0, 1,  // Bottom front edge
			1, 2,  // Right front edge
			2, 3,  // Top front edge
			3, 0,  // Left front edge
			4, 5,  // Bottom back edge
			5, 6,  // Right back edge
			6, 7,  // Top back edge
			7, 4,  // Left back edge
			0, 4,  // Bottom left edge
			1, 5,  // Bottom right edge
			2, 6,  // Top right edge
			3, 7   // Top left edge
		};

		// Create a new ArrayMesh
		var mesh = new ArrayMesh();

		// Create an array to hold the mesh data
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = vertices;
		arrays[(int)Mesh.ArrayType.Index] = indices;

		// Add the surface to the mesh
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);

		// Assign the mesh to the MeshInstance
		Mesh = mesh;
	}
}