using System;

namespace Minecraft.scripts.engine;

public static class Blocks
{
    public enum DefaultBlock {
        Air, Stone, Dirt, Grass, Water
    }

    public enum BlockSide {
        Up, Down, Side
    }

    public static bool IsSolid(ushort block)
    {
        return block != (ushort) DefaultBlock.Air && block != (ushort) DefaultBlock.Water;
    }

    public static bool IsTransparent(ushort block)
    {
        return block == (ushort)DefaultBlock.Water;
    }

    public static int GetTextureIndex(ushort block, MeshUtils.FaceDirection direction)
    {
        BlockSide side = direction switch
        {
            MeshUtils.FaceDirection.Up => BlockSide.Up,
            MeshUtils.FaceDirection.Down => BlockSide.Down,
            MeshUtils.FaceDirection.North
                or MeshUtils.FaceDirection.East
                or MeshUtils.FaceDirection.South
                or MeshUtils.FaceDirection.West => BlockSide.Side,
            _ => throw new ArgumentOutOfRangeException()
        };
        return block switch
        {
            1 => 3,
            2 => 1,
            3 => side switch
            {
                BlockSide.Up => 0,
                BlockSide.Side => 2,
                BlockSide.Down => 1,
                _ => throw new ArgumentOutOfRangeException()
            },
            4 => 4,
            _ => -1
        };
    }
}