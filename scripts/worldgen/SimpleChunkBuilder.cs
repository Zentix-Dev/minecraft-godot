using Godot;
using Minecraft.scripts.engine;

namespace Minecraft.scripts.worldgen;

[GlobalClass]
public partial class SimpleChunkBuilder : ChunkBuilder
{
    [Export] public int Height = 20;
    [Export] public Blocks.DefaultBlock TopBlock;
    [Export] public Blocks.DefaultBlock MiddleBlock;
    [Export] public Blocks.DefaultBlock BottomBlock;

    public override void GenerateChunk(Chunk chunk)
    {
        chunk.FillLayer(0, (ushort)BottomBlock);
        for (int i = 1; i < Height + 1; i++)
            chunk.FillLayer(i, (ushort)MiddleBlock);
        chunk.FillLayer(Height + 1, (ushort)TopBlock);
    }
}