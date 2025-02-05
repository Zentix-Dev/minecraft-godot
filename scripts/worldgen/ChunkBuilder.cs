using Godot;
using Minecraft.scripts.engine;

namespace Minecraft.scripts.worldgen;

[GlobalClass]
public abstract partial class ChunkBuilder : Resource
{
    public abstract void GenerateChunk(Chunk chunk);
}