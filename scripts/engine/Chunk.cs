using System;
using System.Collections.Generic;
using System.Threading;
using Godot;
using Minecraft.scripts.worldgen;

namespace Minecraft.scripts.engine;

public partial class Chunk : MeshInstance3D
{
    public static readonly Vector3I Size = new Vector3I(16, 256, 16);
    
    public ushort[,,] Blocks = new ushort[Size.X, Size.Y, Size.Z];

    [Export] private CollisionShape3D _collisionShape;
    
    public ChunkManager ChunkManager;
    public Vector2I ChunkPos;

    [Signal]
    public delegate void ChunkBuildEventHandler();

    private List<Vector3> _vertices;
    private List<int> _triangles;
    private List<Vector3> _normals;
    private List<Vector2> _uvs;

    private static LinkedList<Chunk> _updateQueue = new();
    private static Chunk _currentUpdatingChunk;
    private static bool _isDone = true;
    private bool _isWaiting;
    private bool _isUpdating;

    private bool _isBuilt;

    private void AddFace(MeshUtils.FaceDirection direction, Vector3I position, ushort block)
    {
        int textureIndex = engine.Blocks.GetTextureIndex(block, direction);
        MeshUtils.CreateFace(direction, position, textureIndex, _vertices, _triangles, _normals, _uvs);
    }

    private void CreateCollider()
    {
        _collisionShape.Shape = Mesh.CreateConvexShape();
    }

    private void CreateMesh()
    {
        Mesh = null;
        Mesh = MeshUtils.CreateArrayMesh(_vertices.ToArray(), _triangles.ToArray(), _uvs.ToArray(), _normals.ToArray());
        CreateCollider();

        if (!_isBuilt)
            EmitSignal(SignalName.ChunkBuild);
        _isBuilt = true;
    }

    public void FillLayer(int layer, ushort block)
    {
        for (int x = 0; x < Size.X; x++)
        for (int z = 0; z < Size.Z; z++) Blocks[x, layer, z] = block;
    }

    public ushort GetBlock(Vector3I pos)
    {
        if (pos.Y >= Size.Y || pos.Y < 0)
            return 0;
        if (pos.X >= Size.X)
            if (ChunkManager?.Chunks.TryGetValue(ChunkPos + Vector2I.Right, out Chunk neighbor) == true)
                return neighbor.GetBlock(pos - new Vector3I(Size.X, 0, 0));
        if (pos.Z >= Size.Z)
            if (ChunkManager?.Chunks.TryGetValue(ChunkPos + Vector2I.Down, out Chunk neighbor) == true)
                return neighbor.GetBlock(pos - new Vector3I(0, 0, Size.Z));
        if (pos.X < 0)
            if (ChunkManager?.Chunks.TryGetValue(ChunkPos + Vector2I.Left, out Chunk neighbor) == true)
                return neighbor.GetBlock(pos + new Vector3I(Size.X, 0, 0));
        if (pos.Z < 0)
            if (ChunkManager?.Chunks.TryGetValue(ChunkPos + Vector2I.Up, out Chunk neighbor) == true)
                return neighbor.GetBlock(pos + new Vector3I(0, 0, Size.Z));
        if (pos.X >= Size.X || pos.Z >= Size.Z || pos.X < 0 || pos.Z < 0)
            return 0;
        return Blocks[pos.X, pos.Y, pos.Z];
    }

    public void SetBlock(Vector3I pos, ushort block)
    {
        if (pos.Y >= Size.Y || pos.Y < 0 || pos.X >= Size.X || pos.X < 0 || pos.Z >= Size.Z || pos.Z < 0)
            return;
        Blocks[pos.X, pos.Y, pos.Z] = block;
    }

    private void UpdateCube(Vector3I pos)
    {
        ushort block = GetBlock(pos);
        if (!engine.Blocks.IsSolid(block))
            return;
        // TODO: Fix GetValues memory performance issues
        foreach (MeshUtils.FaceDirection direction in Enum.GetValues<MeshUtils.FaceDirection>())
        {
            Vector3I vector = MeshUtils.GetDirectionVector(direction);
            if (!engine.Blocks.IsSolid(GetBlock(pos + vector)))
                AddFace(direction, pos, block);
        }
    }

    public static void UpdateMeshes()
    {
        if (_isDone)
        {
            if (_currentUpdatingChunk != null)
            {
                _currentUpdatingChunk.CreateMesh();
                _currentUpdatingChunk = null;
            }
            if (_updateQueue.Count > 0)
            {
                _currentUpdatingChunk = _updateQueue.First!.Value;
                _updateQueue.RemoveFirst();
                _isDone = false;
                _currentUpdatingChunk.UpdateMeshNow();
            }
        }
    }

    public int GetHeightAt(Vector2I pos)
    {
        for (int z = Size.Y - 1; z >= 0; z--)
        {
            if (engine.Blocks.IsSolid(GetBlock(new Vector3I(pos.X, z, pos.Y))))
                return z + 1;
        }

        return 0;
    }

    private void UpdateMeshNow()
    {
        _isUpdating = true;
        _isWaiting = false;
        
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _normals = new List<Vector3>();
        _uvs = new List<Vector2>();

        new Thread(() =>
        {
            _isDone = false;
            for (int y = 0; y < Size.Y; y++)
            for (int x = 0; x < Size.X; x++)
            for (int z = 0; z < Size.Z; z++)
                UpdateCube(new Vector3I(x, y, z));
            _isDone = true;
            _isUpdating = false;
        }).Start();
    }

    public void UpdateMeshImmediate()
    {
        if (_isWaiting) return;
        if (_isUpdating) _isWaiting = true;
        _updateQueue.AddFirst(this);
    }

    public void UpdateMesh()
    {
        if (_isWaiting) return;
        if (_isUpdating) _isWaiting = true;
        _updateQueue.AddLast(this);
    }
}