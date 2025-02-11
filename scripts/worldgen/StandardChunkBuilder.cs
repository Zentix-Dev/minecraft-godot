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

            if (GD.Randf() > 1 - 1 / 1000f)
            {
                int treeHeight = GD.RandRange(4, 7);
                for (int y = terrainHeight; y < terrainHeight + treeHeight; y++)
                {
                    chunk.SetBlock(new Vector3I(x, y, z), (ushort) Blocks.DefaultBlock.Log);
                }
            }
        }
    }
}