using Godot;
using Minecraft.scripts.engine;
using Minecraft.scripts.worldgen;

namespace Minecraft.scripts.game;

public partial class BlockBreaker : RayCast3D
{
    [Export] private CubeSelection _selectionCube;
    [Export] private BlockPicker _blockPicker;

    private bool _needsUpdate = false;

    private void UpdateBlockSelection()
    {
        _needsUpdate = true;
    }

    private void UpdateBlockSelectionNow()
    {
        Vector3 worldPosHit = (GetCollisionPoint() - GetCollisionNormal() * .05f).Round();
        _selectionCube.GlobalPosition = worldPosHit.Round();
    }

    private void SetTargetBlock(bool destroy, ushort block = 0)
    {
        Vector3 worldPosHit = (GetCollisionPoint() - GetCollisionNormal() * (destroy ? .05f : -.05f)).Round();
            
        var chunkManager = ChunkManager.Instance;
        Chunk chunk = chunkManager.GetChunkAt(worldPosHit);
        Vector2I chunkPos = chunkManager.GetChunkPosAt(worldPosHit);
        Vector3I posInChunk = chunkManager.GetPosInChunk(worldPosHit);
        GD.Print($"Destroying block: {chunk.GetBlock(posInChunk)} at pos {posInChunk} of chunk {chunkPos}");
        chunk.SetBlock(posInChunk, destroy ? (ushort)Blocks.DefaultBlock.Air : block);
        chunk.UpdateNeighborsImmediate();
        chunk.UpdateMeshImmediate();
        CallDeferred(nameof(UpdateBlockSelection));
    }

    public override void _Process(double delta)
    {
        if (!_needsUpdate) return;
        UpdateBlockSelectionNow();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        _selectionCube.Visible = IsColliding();
        
        if (!IsColliding()) return;

        if (@event is InputEventMouseMotion)
           UpdateBlockSelection();
        
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left or MouseButton.Right } mouseButtonEvent)
        {
            bool isDestroy = mouseButtonEvent.ButtonIndex == MouseButton.Left;
            SetTargetBlock(isDestroy, (ushort)_blockPicker.SelectedBlock);
        }
    }
}