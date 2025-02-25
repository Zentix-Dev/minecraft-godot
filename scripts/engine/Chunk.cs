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

    [Export] public CollisionShape3D CollisionShape;
    [Export] private MeshInstance3D _transparentMeshInstance;
    
    public ChunkManager ChunkManager;
    public Vector2I ChunkPos;

    [Signal]
    public delegate void ChunkBuildEventHandler();

    private MeshArrays _solidMesh;
    private MeshArrays _transparentMesh;

    private List<Vector3> _collisionFaces;
    
    private static readonly Dictionary<Vector2I, Dictionary<Vector3I, ushort>> UngeneratedBlocks = new();

    private static readonly LinkedList<Chunk> UpdateQueue = new();
    private static Chunk _currentUpdatingChunk;
    private static bool _isDone = true;
    private bool _isWaiting;
    private bool _isUpdating;

    private bool _isBuilt;

    private readonly MeshUtils.FaceDirection[] _faceDirections = Enum.GetValues<MeshUtils.FaceDirection>();

    private void AddCollisionFace(MeshUtils.FaceDirection direction, Vector3I position)
    {
        List<Vector3> verts = null;
        switch (direction)
        {
            case MeshUtils.FaceDirection.North:
            case MeshUtils.FaceDirection.South:
            {
                int dir = direction == MeshUtils.FaceDirection.North ? 1 : -1;
                verts = new List<Vector3>
                {
                    new Vector3(0.5f, 0.5f, 0.5f * dir) + position,
                    new Vector3(0.5f, -0.5f, 0.5f * dir) + position,
                    new Vector3(-0.5f, -0.5f, 0.5f * dir) + position,
                    new Vector3(-0.5f, 0.5f, 0.5f * dir) + position
                };
                if (dir == -1)
                    verts.Reverse();
                break;
            }
            case MeshUtils.FaceDirection.East:
            case MeshUtils.FaceDirection.West:
            {
                int dir = direction == MeshUtils.FaceDirection.East ? 1 : -1;
                verts = new List<Vector3>
                {
                    new Vector3(0.5f * dir, 0.5f, -0.5f) + position,
                    new Vector3(0.5f * dir, -0.5f, -0.5f) + position,
                    new Vector3(0.5f * dir, -0.5f, 0.5f) + position,
                    new Vector3(0.5f * dir, 0.5f, 0.5f) + position
                };
                if (dir == -1)
                    verts.Reverse();
                break;
            }
            case MeshUtils.FaceDirection.Up:
            case MeshUtils.FaceDirection.Down:
            {
                int dir = direction == MeshUtils.FaceDirection.Up ? 1 : -1;
                verts = new List<Vector3>
                {
                    new Vector3(-0.5f, 0.5f * dir, 0.5f) + position,
                    new Vector3(-0.5f, 0.5f * dir, -0.5f) + position,
                    new Vector3(0.5f, 0.5f * dir, -0.5f) + position,
                    new Vector3(0.5f, 0.5f * dir, 0.5f) + position
                };
                if (dir == -1)
                    verts.Reverse();
                break;
            }
        }
        
        _collisionFaces.Add(verts![0]);
        _collisionFaces.Add(verts[1]);
        _collisionFaces.Add(verts[2]);
                
        _collisionFaces.Add(verts[2]);
        _collisionFaces.Add(verts[3]);
        _collisionFaces.Add(verts[0]);
    }

    private void AddFace(MeshUtils.FaceDirection direction, Vector3I position, ushort block, MeshArrays mesh)
    {
        int textureIndex = engine.Blocks.GetTextureIndex(block, direction);
        MeshUtils.CreateFace(direction, position, textureIndex, mesh.Vertices, mesh.Triangles, mesh.Normals, mesh.Uvs);
        if (engine.Blocks.IsColliding(block))
            AddCollisionFace(direction, position);
    }

    private void CreateCollider()
    {
        var shape = new ConcavePolygonShape3D();
        shape.SetFaces(_collisionFaces.ToArray());
        CollisionShape.Shape = shape;
    }

    private void CreateMesh()
    {
        Mesh = null;
        Mesh = MeshUtils.CreateArrayMesh(_solidMesh.Vertices.ToArray(), _solidMesh.Triangles.ToArray(), _solidMesh.Uvs.ToArray(), _solidMesh.Normals.ToArray());

        _transparentMeshInstance.Mesh = null;
        if (_transparentMesh.Vertices.Count > 0)
        {
            _transparentMeshInstance.Mesh = MeshUtils.CreateArrayMesh(_transparentMesh.Vertices.ToArray(), _transparentMesh.Triangles.ToArray(), _transparentMesh.Uvs.ToArray(), _transparentMesh.Normals.ToArray());
        }
        
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

    private void AddUngeneratedBlock(Vector2I chunkPos, Vector3I pos, ushort block)
    {
        if (!UngeneratedBlocks.ContainsKey(chunkPos))
            UngeneratedBlocks[chunkPos] = new Dictionary<Vector3I, ushort>();
        var chunkDictionary = UngeneratedBlocks[chunkPos];
        chunkDictionary[pos] = block;
    }

    public Dictionary<Vector3I, ushort> GetUngeneratedBlocks()
    {
        return UngeneratedBlocks.GetValueOrDefault(ChunkPos);
    }

    public void ClearUngeneratedBlocks()
    {
        UngeneratedBlocks.Remove(ChunkPos);
    }

    public void SetBlock(Vector3I pos, ushort block)
    {
        if (pos.Y >= Size.Y || pos.Y < 0)
            return;
        if (pos.X >= Size.X)
        {
            if (ChunkManager?.Chunks.TryGetValue(ChunkPos + Vector2I.Right, out Chunk neighbor) == true)
                neighbor.SetBlock(pos - new Vector3I(Size.X, 0, 0), block);
            else
                AddUngeneratedBlock(ChunkPos + Vector2I.Right, pos - new Vector3I(Size.X, 0, 0), block);
            return;
        }
        if (pos.Z >= Size.Z)
        {
            if (ChunkManager?.Chunks.TryGetValue(ChunkPos + Vector2I.Down, out Chunk neighbor) == true)
                neighbor.SetBlock(pos - new Vector3I(0, 0, Size.Z), block);
            else
                AddUngeneratedBlock(ChunkPos + Vector2I.Down, pos - new Vector3I(0, 0, Size.Z), block);
            return;
        }
        if (pos.X < 0)
        {
            if (ChunkManager?.Chunks.TryGetValue(ChunkPos + Vector2I.Left, out Chunk neighbor) == true)
                neighbor.SetBlock(pos + new Vector3I(Size.X, 0, 0), block);
            else
                AddUngeneratedBlock(ChunkPos + Vector2I.Left, pos + new Vector3I(Size.X, 0, 0), block);
            return;
        }
        if (pos.Z < 0)
        {
            if (ChunkManager?.Chunks.TryGetValue(ChunkPos + Vector2I.Up, out Chunk neighbor) == true)
                neighbor.SetBlock(pos + new Vector3I(0, 0, Size.Z), block);
            AddUngeneratedBlock(ChunkPos + Vector2I.Up, pos + new Vector3I(0, 0, Size.Z), block);
            return;
        }
        
        Blocks[pos.X, pos.Y, pos.Z] = block;
    }

    private void UpdateCube(Vector3I pos)
    {
        ushort block = GetBlock(pos);
        foreach (MeshUtils.FaceDirection direction in _faceDirections)
        {
            Vector3I vector = MeshUtils.GetDirectionVector(direction);
            if (engine.Blocks.IsTransparent(block))
            {
                var neighborBlock = GetBlock(pos + vector);
                if (neighborBlock == (ushort)engine.Blocks.DefaultBlock.Air ||
                    (block == (ushort)engine.Blocks.DefaultBlock.Leaves &&
                     neighborBlock == (ushort)engine.Blocks.DefaultBlock.Leaves))
                    AddFace(direction, pos, block, _transparentMesh);
            }
            else
            {
                if (!engine.Blocks.IsSolid(block))
                    return;
                if (!engine.Blocks.IsSolid(GetBlock(pos + vector)))
                    AddFace(direction, pos, block, _solidMesh);
            }
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
            if (UpdateQueue.Count > 0)
            {
                _currentUpdatingChunk = UpdateQueue.First!.Value;
                UpdateQueue.RemoveFirst();
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

        _transparentMesh = new MeshArrays();
        _solidMesh = new MeshArrays();
        _collisionFaces = new List<Vector3>();

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

    public void UpdateNeighbors()
    {
        if (ChunkManager.Chunks.TryGetValue(ChunkPos + Vector2I.Right, out Chunk neighborRight))
            neighborRight.UpdateMesh();
        if (ChunkManager.Chunks.TryGetValue(ChunkPos + Vector2I.Left, out Chunk neighborLeft))
            neighborLeft.UpdateMesh();
        if (ChunkManager.Chunks.TryGetValue(ChunkPos + Vector2I.Up, out Chunk neighborUp))
            neighborUp.UpdateMesh();
        if (ChunkManager.Chunks.TryGetValue(ChunkPos + Vector2I.Down, out Chunk neighborDown))
            neighborDown.UpdateMesh();
    }
    
    public void UpdateNeighborsImmediate()
    {
        if (ChunkManager.Chunks.TryGetValue(ChunkPos + Vector2I.Right, out Chunk neighborRight))
            neighborRight.UpdateMeshImmediate();
        if (ChunkManager.Chunks.TryGetValue(ChunkPos + Vector2I.Left, out Chunk neighborLeft))
            neighborLeft.UpdateMeshImmediate();
        if (ChunkManager.Chunks.TryGetValue(ChunkPos + Vector2I.Up, out Chunk neighborUp))
            neighborUp.UpdateMeshImmediate();
        if (ChunkManager.Chunks.TryGetValue(ChunkPos + Vector2I.Down, out Chunk neighborDown))
            neighborDown.UpdateMeshImmediate();
    }

    public void UpdateMeshImmediate()
    {
        if (_isWaiting) return;
        if (_isUpdating) _isWaiting = true;
        UpdateQueue.AddFirst(this);
    }

    public void UpdateMesh()
    {
        if (_isWaiting) return;
        if (_isUpdating) _isWaiting = true;
        UpdateQueue.AddLast(this);
    }
}