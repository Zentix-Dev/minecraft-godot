using Godot;
using Minecraft.scripts.worldgen;

namespace Minecraft;

public partial class ChunkDebugger : Label
{
	[Export] public Node3D Player;
	[Export] public ChunkManager ChunkManager;

	public override void _Process(double delta)
	{
		Text =
			$"Chunk: {ChunkManager.GetChunkPosAt(Player.GlobalPosition)}; Pos in chunk: {ChunkManager.GetPosInChunk(Player.GlobalPosition)}; Position: {Player.GlobalPosition}";
	}
}