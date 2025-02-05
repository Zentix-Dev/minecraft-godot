using System.Collections.Generic;
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

    public Chunk BuildChunk(Vector2I position)
    {
        var chunk = _chunkScene.Instantiate<Chunk>();
        AddChild(chunk);
        chunk.Position = new Vector3(position.X * Chunk.Size.X, 0, position.Y * Chunk.Size.Z);
        chunk.ChunkPos = position;
        chunk.ChunkManager = this;
        
        Chunks.Add(position, chunk);
        chunk.UpdateNeighbors();
        
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
        return (Vector2I)(new Vector2(worldPosition.X, worldPosition.Z).Round() / new Vector2(Chunk.Size.Z, Chunk.Size.Z)).Floor();
    }

    public Vector3I GetPosInChunk(Vector3 worldPosition)
    {
        worldPosition.Y = Mathf.Clamp(worldPosition.Y, 0, Chunk.Size.Y);
        Vector3 chunkPos = (worldPosition.Round() / Chunk.Size) % 1;
        Vector3 posInChunk = (chunkPos * Chunk.Size).Floor();
        return new Vector3I((int)(posInChunk.X < 0 ? 16 + posInChunk.X : posInChunk.X), (int)posInChunk.Y, (int)(posInChunk.Z < 0 ? 16 + posInChunk.Z : posInChunk.Z));
    }

    public Chunk GetChunkAt(Vector3 worldPosition) => Chunks.GetValueOrDefault(GetChunkPosAt(worldPosition));

    private void SetChunkVisible(Chunk chunk, bool visible)
    {
        chunk.Visible = visible;
        chunk.CollisionShape.Disabled = !visible;
    }
    
    public override void _Process(double delta)
    {
        var chunkPosition = GetChunkPosAt(Viewer.GlobalPosition);
        var visibleChunks = new HashSet<Vector2I>();
        for (int x = chunkPosition.X - RenderDistance / 2; x < chunkPosition.X + RenderDistance / 2; x++)
        {
            for (int y = chunkPosition.Y - RenderDistance / 2; y < chunkPosition.Y + RenderDistance / 2; y++)
            {
                if (Chunks.TryGetValue(new Vector2I(x, y), out Chunk existingChunk))
                    SetChunkVisible(existingChunk, true);
                else
                    BuildChunk(new Vector2I(x, y));
                visibleChunks.Add(new Vector2I(x, y));
            }
        }
        foreach ((Vector2I pos, Chunk chunk) in Chunks)
        {
            if (visibleChunks.Contains(pos))
                continue;
            SetChunkVisible(chunk, false);
        }
        
        Chunk.UpdateMeshes();
    }
}