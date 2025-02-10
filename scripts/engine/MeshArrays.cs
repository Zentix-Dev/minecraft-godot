using System.Collections.Generic;
using Godot;

namespace Minecraft.scripts.engine;

public class MeshArrays
{
    public List<Vector3> Vertices = new();
    public List<int> Triangles = new();
    public List<Vector3> Normals = new();
    public List<Vector2> Uvs = new();
}