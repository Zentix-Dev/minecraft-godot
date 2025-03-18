using System;
using System.Collections.Generic;
using Godot;

namespace Minecraft.scripts.engine;

public static class PreviewBlockGenerator
{
    public static ArrayMesh GenerateBlockMesh(Blocks.DefaultBlock block)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        var values = Enum.GetValues<MeshUtils.FaceDirection>();
        foreach (var faceDirection in values)
            MeshUtils.CreateFace(faceDirection,
                Vector3.Zero,
                Blocks.GetTextureIndex((ushort)block,
                    faceDirection),
                vertices,
                triangles,
                normals,
                uvs);
        return MeshUtils.CreateArrayMesh(vertices.ToArray(), triangles.ToArray(), uvs.ToArray(), normals.ToArray());
    }
}