using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Minecraft.scripts.engine;

namespace Minecraft.scripts.worldgen;

public partial class ChunkManager : Node3D
{
    public static ChunkManager Instance;
    
    [Export] private PackedScene _chunkScene;
    [Export] public ChunkBuilder ChunkBuilder;
    [Export] public Node3D Viewer;
    [Export] public int RenderDistance;
    
    public Dictionary<Vector2I, Chunk> Chunks = new();

    [Signal]
    public delegate void ChunkAddedEventHandler(Chunk chunk);
    
    [Signal]
    public delegate void ChunkGeneratedEventHandler(Chunk chunk, ChunkBuilder chunkBuilder);

    [Signal]
    public delegate void InitialChunksLoadedEventHandler();

    private void UpdateNeighbors(Vector2I chunkPosition)
    {
        if (Chunks.TryGetValue(chunkPosition + Vector2I.Left, out Chunk chunkLeft))
            chunkLeft.UpdateMesh();
        if (Chunks.TryGetValue(chunkPosition + Vector2I.Right, out Chunk chunkRight))
            chunkRight.UpdateMesh();
        if (Chunks.TryGetValue(chunkPosition + Vector2I.Up, out Chunk chunkUp))
            chunkUp.UpdateMesh();
        if (Chunks.TryGetValue(chunkPosition + Vector2I.Down, out Chunk chunkDown))
            chunkDown.UpdateMesh();
    }

    public Chunk BuildChunk(Vector2I position)
    {
        var chunk = _chunkScene.Instantiate<Chunk>();
        AddChild(chunk);
        chunk.Position = new Vector3(position.X * Chunk.Size.X, 0, position.Y * Chunk.Size.Z);
        chunk.ChunkPos = position;
        chunk.ChunkManager = this;
        
        Chunks.Add(position, chunk);
        UpdateNeighbors(position);
        
        EmitSignal(SignalName.ChunkAdded, chunk);
        
        ChunkBuilder.GenerateChunk(chunk);

        EmitSignal(SignalName.ChunkGenerated, chunk, ChunkBuilder);
        
        chunk.UpdateMesh();
        return chunk;
    }

    public override void _Ready()
    {
        Instance = this;
        
        int chunksBuilt = 0;
        int chunksNeedBuilt = 8 * 8;
        
        for (int x = -4; x < 4; x++)
        for (int z = -4; z < 4; z++)
        {
            
            var chunk = BuildChunk(new Vector2I(x, z));
            chunk.ChunkBuild += () =>
            {
                chunksBuilt++;
                if (chunksBuilt >= chunksNeedBuilt)
                    EmitSignal(SignalName.InitialChunksLoaded);
            };
        }

        foreach ((Vector2I key, Chunk value) in Chunks)
        {
            value.UpdateMesh();
        }
    }

    public Vector2I GetChunkPosAt(Vector3 worldPosition)
    {
        return new Vector2I((int)worldPosition.X, (int)worldPosition.Z) /
                                 new Vector2I(Chunk.Size.X, Chunk.Size.Z);
    }

    public Vector2I GetPosInChunk(Vector3 worldPosition, Vector2I chunkPosition)
    {
        return new Vector2I((int)worldPosition.X, (int)worldPosition.Z) - chunkPosition * new Vector2I(Chunk.Size.X, Chunk.Size.Z);
    }

    public Chunk GetChunkAt(Vector3 worldPosition) => Chunks.GetValueOrDefault(GetChunkPosAt(worldPosition));

    public override void _Process(double delta)
    {
        var chunkPosition = GetChunkPosAt(Viewer.GlobalPosition);
        var visibleChunks = new HashSet<Vector2I>();
        for (int x = chunkPosition.X - RenderDistance / 2; x < chunkPosition.X + RenderDistance / 2; x++)
        {
            for (int y = chunkPosition.Y - RenderDistance / 2; y < chunkPosition.Y + RenderDistance / 2; y++)
            {
                if (Chunks.TryGetValue(new Vector2I(x, y), out Chunk existingChunk))
                    existingChunk.Visible = true;
                else
                    BuildChunk(new Vector2I(x, y));
                visibleChunks.Add(new Vector2I(x, y));
            }
        }
        foreach ((Vector2I pos, Chunk chunk) in Chunks)
        {
            if (visibleChunks.Contains(pos))
                continue;
            chunk.Visible = false;
        }
        
        Chunk.UpdateMeshes();
    }
}