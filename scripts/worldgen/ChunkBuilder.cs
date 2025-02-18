using Godot;
using Minecraft.scripts.engine;

namespace Minecraft.scripts.worldgen;

[GlobalClass]
public abstract partial class ChunkBuilder : Resource
{
    public abstract void GenerateChunk(Chunk chunk);

    protected void PlaceUngeneratedBlocks(Chunk chunk)
    {
        var ungeneratedBlocks = chunk.GetUngeneratedBlocks();
        if (ungeneratedBlocks == null) return;
        
        foreach (var (pos, block) in ungeneratedBlocks)
        {
            chunk.SetBlock(pos, block);
        }
        chunk.ClearUngeneratedBlocks();
    }
}