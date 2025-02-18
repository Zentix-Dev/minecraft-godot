using Godot;
using Minecraft.scripts.engine;

namespace Minecraft.scripts.worldgen;

[GlobalClass]
public partial class StandardChunkBuilder : ChunkBuilder
{
    [ExportGroup("Terrain Parameters"), Export]
    public Blocks.DefaultBlock TopBlock;
    [Export]
    public Blocks.DefaultBlock MiddleBlock;
    [Export]
    public Blocks.DefaultBlock BottomBlock;
    [Export]
    public int BottomBlockHeight;

    [Export]
    public int TerrainHeightMin;
    [Export]
    public int TerrainHeightMax;
    [Export] 
    public int WaterHeight;
    
    [ExportGroup("Noise Parameters"), Export]
    public Vector3 Offset;

    [Export] public Vector2 Scale = Vector2.One;

    [Export] public int Octaves = 10;
    [Export] public float Lacunarity = 2f;
    [Export] public string Seed = "0";

    private static Chunk _currentChunk;
    private static Vector3I _currentOffset;
    
    private void PlaceTreeBlock(Vector3I offset, Blocks.DefaultBlock block)
    {
        _currentChunk.SetBlock(_currentOffset + offset, (ushort)block);
    }

    private void GenerateTree(Vector3I position)
    {
        int treeHeight = GD.RandRange(4, 7);

        _currentOffset = position + new Vector3I(0, treeHeight - 2, 0);

        for (int x = -2; x < 3; x++)
        for (int z = -2; z < 3; z++)
        for (int y = 0; y < 2; y++)
        {
            if (x is -2 or 2 && z is -2 or 2 && GD.Randf() > .5f)
                continue;
            PlaceTreeBlock(new Vector3I(x, y, z), Blocks.DefaultBlock.Leaves);
        }

        _currentOffset += new Vector3I(0, 2, 0);

        for (int x = -1; x < 2; x++)
        for (int z = -1; z < 2; z++)
        {
            int height = x is -1 or 1 && z is -1 or 2 ? GD.RandRange(0, 2) : 2;
            for (int y = 0; y < height; y++)
            {
                PlaceTreeBlock(new Vector3I(x, y, z), Blocks.DefaultBlock.Leaves);
            }
        }

        _currentOffset = position;
        
        for (int y = 0; y < treeHeight; y++)
        {
            PlaceTreeBlock(new Vector3I(0, y, 0), Blocks.DefaultBlock.Log);
        }
    }
    
    public override void GenerateChunk(Chunk chunk)
    {
        var noiseGenerator = new FastNoiseLite()
        {

            Offset = Offset,
            FractalLacunarity = Lacunarity,
            FractalOctaves = Octaves,
            Seed = Seed.GetHashCode(),
            NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex
        };
        
        for (int x = 0; x < Chunk.Size.X; x++)
        for (int z = 0; z < Chunk.Size.Z; z++)
        {
            var worldPosition = new Vector2I(x + chunk.ChunkPos.X * Chunk.Size.X, z + chunk.ChunkPos.Y * Chunk.Size.Z);
            float height = Mathf.InverseLerp(-1, 1, noiseGenerator.GetNoise2D(worldPosition.X + Scale.X, worldPosition.Y * Scale.Y));
            int terrainHeight = Mathf.RoundToInt(Mathf.Lerp(TerrainHeightMin, TerrainHeightMax, height));
            for (int y = 0; y < terrainHeight; y++)
            {
                if (y == terrainHeight - 1)
                    chunk.SetBlock(new Vector3I(x, y, z), (ushort)TopBlock);
                else if (y < BottomBlockHeight)
                    chunk.SetBlock(new Vector3I(x, y, z), (ushort)BottomBlock);
                else
                    chunk.SetBlock(new Vector3I(x, y, z), (ushort)MiddleBlock);
            }

            for (int y = terrainHeight; y < WaterHeight; y++)
            {
                chunk.SetBlock(new Vector3I(x, y, z), (ushort)Blocks.DefaultBlock.Water);
            }

            if (terrainHeight < WaterHeight)
                continue;

            _currentChunk = chunk;

            if (GD.Randf() > 1 - 1 / 1000f)
            {
                GenerateTree(new Vector3I(x, terrainHeight, z));
            }
        }
        
        PlaceUngeneratedBlocks(chunk);
    }
}