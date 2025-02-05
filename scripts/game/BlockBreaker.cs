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
            Vector3 worldPosHit = (GetCollisionPoint() - GetCollisionNormal() * .05f).Round();
            
            var chunkManager = ChunkManager.Instance;
            Chunk chunk = chunkManager.GetChunkAt(worldPosHit);
            Vector2I chunkPos = chunkManager.GetChunkPosAt(worldPosHit);
            Vector3I posInChunk = chunkManager.GetPosInChunk(worldPosHit);
            GD.Print($"Destroying block: {chunk.GetBlock(posInChunk)} at pos {posInChunk} of chunk {chunkPos}");
            chunk.SetBlock(posInChunk, (ushort)Blocks.DefaultBlock.Air);
            chunk.UpdateMeshImmediate();
        }
    }
}