using System;
using System.Collections.Generic;
using Godot;

namespace Minecraft.scripts.engine;

public static class MeshUtils
{
	public const int TextureWidth = 256;
	public const int TextureHeight = 256;
	public const int TextureRows = 16;
	public const int TextureColumns = 16;

	public enum FaceDirection
	{
		North, South, East, West, Up, Down
	}

	public static Vector3I GetDirectionVector(FaceDirection faceDirection)
	{
		return faceDirection switch
		{
			FaceDirection.North => Vector3I.Back,
			FaceDirection.South => Vector3I.Forward,
			FaceDirection.East => Vector3I.Right,
			FaceDirection.West => Vector3I.Left,
			FaceDirection.Up => Vector3I.Up,
			FaceDirection.Down => Vector3I.Down,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public static ArrayMesh CreateArrayMesh(Vector3[] vertices, int[] indices, Vector2[] uvs, Vector3[] normals)
	{
		var mesh = new ArrayMesh();
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = vertices;
		arrays[(int)Mesh.ArrayType.Index] = indices;
		arrays[(int)Mesh.ArrayType.TexUV] = uvs;
		arrays[(int)Mesh.ArrayType.Normal] = normals;
		
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		return mesh;
	}

	public static Vector2[] GetTextureUVs(int textureIndex)
	{
		//TODO: Fix uv map issue with water block
		float width = (float)TextureWidth / TextureColumns / TextureWidth;
		float height = (float)TextureHeight / TextureRows / TextureHeight;
		float x = textureIndex % TextureColumns * width;
		float y = Mathf.Floor(textureIndex / (float)TextureColumns) * height;
		return new Vector2[]
		{
			new(x + width, y),
			new(x + width, y + height),
			new(x, y + height),
			new(x, y)
		};
	}
	
	public static void CreateFace(FaceDirection direction, Vector3 offset, int textureIndex, List<Vector3> vertices, List<int> indices, List<Vector3> normals, List<Vector2> uvs)
    {
        int arrayOffset = vertices.Count;

        switch (direction)
        {
            case FaceDirection.North:
            case FaceDirection.South:
            {
	            int dir = direction == FaceDirection.North ? 1 : -1;
	            // TODO: Fix list performance issues
	            var verts = new List<Vector3>
	            {
		            new Vector3(0.5f, 0.5f, 0.5f * dir) + offset,
		            new Vector3(0.5f, -0.5f, 0.5f * dir) + offset,
		            new Vector3(-0.5f, -0.5f, 0.5f * dir) + offset,
		            new Vector3(-0.5f, 0.5f, 0.5f * dir) + offset
	            };
	            var normal = new Vector3(0, 0, dir);
	            // TODO: Fix AddRange performance issues
	            normals.AddRange(new[] { normal, normal, normal, normal });
	            if (dir == -1)
	            {
		            verts.Reverse();
	            }

	            vertices.AddRange(verts);
	            break;
            }

            case FaceDirection.East:
            case FaceDirection.West:
            {
	            int dir = direction == FaceDirection.East ? 1 : -1;
	            var verts = new List<Vector3>
	            {
		            new Vector3(0.5f * dir, 0.5f, -0.5f) + offset,
		            new Vector3(0.5f * dir, -0.5f, -0.5f) + offset,
		            new Vector3(0.5f * dir, -0.5f, 0.5f) + offset,
		            new Vector3(0.5f * dir, 0.5f, 0.5f) + offset
	            };
	            var normal = new Vector3(dir, 0, 0);
	            normals.AddRange(new[] { normal, normal, normal, normal });
	            if (dir == -1)
	            {
		            verts.Reverse();
	            }

	            vertices.AddRange(verts);
	            break;
            }

            case FaceDirection.Up:
            case FaceDirection.Down:
            {
	            int dir = direction == FaceDirection.Up ? 1 : -1;
	            var verts = new List<Vector3>
	            {
		            new Vector3(-0.5f, (textureIndex == 4 && dir > 0 ? 0.4f : 0.5f) * dir, 0.5f) + offset,
		            new Vector3(-0.5f, (textureIndex == 4 && dir > 0 ? 0.4f : 0.5f) * dir, -0.5f) + offset,
		            new Vector3(0.5f, (textureIndex == 4 && dir > 0 ? 0.4f : 0.5f) * dir, -0.5f) + offset,
		            new Vector3(0.5f, (textureIndex == 4 && dir > 0 ? 0.4f : 0.5f) * dir, 0.5f) + offset
	            };
	            var normal = new Vector3(0, dir, 0);
	            normals.AddRange(new[] { normal, normal, normal, normal });
	            if (dir == -1)
	            {
		            verts.Reverse();
	            }

	            vertices.AddRange(verts);
	            break;
            }
            default:
	            throw new ArgumentOutOfRangeException();
        }

        indices.AddRange(new[]
        {
            0 + arrayOffset, 1 + arrayOffset, 2 + arrayOffset,
            2 + arrayOffset, 3 + arrayOffset, 0 + arrayOffset
        });

        var uvMappings = GetTextureUVs(textureIndex);
        uvs.AddRange(uvMappings);
    }
}