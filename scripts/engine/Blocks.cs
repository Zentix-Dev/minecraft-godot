using System;

namespace Minecraft.scripts.engine;

public static class Blocks
{
    public enum DefaultBlock {
        Air, Stone, Dirt, Grass, Water, Log, Leaves
    }

    public enum BlockSide {
        Up, Down, Side
    }

    public static bool IsSolid(ushort block)
    {
        return block is not ((ushort) DefaultBlock.Air or (ushort) DefaultBlock.Water or (ushort) DefaultBlock.Leaves);
    }

    public static bool IsTransparent(ushort block)
    {
        return block is (ushort)DefaultBlock.Water or (ushort)DefaultBlock.Leaves;
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
            5 => side switch
            {
                BlockSide.Up => 6,
                BlockSide.Down => 6,
                BlockSide.Side => 5,
                _ => throw new ArgumentOutOfRangeException()
            },
            6 => 7,
            _ => -1
        };
    }
}