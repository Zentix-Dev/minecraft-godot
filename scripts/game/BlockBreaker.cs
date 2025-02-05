using Godot;
using Minecraft.scripts.engine;
using Minecraft.scripts.worldgen;

namespace Minecraft.scripts.game;

public partial class BlockBreaker : RayCast3D
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true })
        {
            GetViewport().SetInputAsHandled();
            Vector3 worldPosHit = GetCollisionPoint().Round();
            var chunkManager = ChunkManager.Instance;
            Chunk chunk = chunkManager.GetChunkAt(worldPosHit);
            Vector2I chunkPos = chunkManager.GetChunkPosAt(worldPosHit);
            Vector3I posInChunk = chunkManager.GetPosInChunk(worldPosHit, chunkPos);
            GD.Print("Destroying block: " + chunk.GetBlock(posInChunk));
            chunk.SetBlock(posInChunk, (ushort)Blocks.DefaultBlock.Air);
            chunk.UpdateMeshImmediate();
        }
    }
}