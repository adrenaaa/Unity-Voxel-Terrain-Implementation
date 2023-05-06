using Unity.Mathematics;
using System.Linq;

// takes a noise map and generates voxel data based on noise
public static class VoxelDataGenerator {
    public static VoxelData GenerateVoxelData(World world, int3 chunkPos) {
        // creates new voxel data
        VoxelData voxelData = new VoxelData(World.sizeX + 1, World.sizeY + 1, World.sizeZ + 1, chunkPos);

        // makes my life more convenient to access the biome manager 
        BiomeManager biomeManager = world.biomeManager;

        int biomeIndex;

        #region Noise Maps

        // where all the noise maps are calculated
        // we use noise maps so we can change between 2d and 3d on the fly

        float[] shapeNoiseMap = NoiseCore.NoiseMap(chunkPos, world.seed, world.shapeNoise, voxelData.size.x, voxelData.size.y, voxelData.size.z);
        float[] shapeNoiseMap2 = NoiseCore.NoiseMap(chunkPos, world.seed, world.shapeNoise2, voxelData.size.x, voxelData.size.y, voxelData.size.z);
        float[] overlayNoiseMap = NoiseCore.NoiseMap(chunkPos, world.seed, world.overlayNoise, voxelData.size.x, voxelData.size.y, voxelData.size.z);

        // we add +1 and -1 to the seeds so the temperature and humidity maps are not relative to the generation
        float[] temperatureNoiseMap = NoiseCore.NoiseMap(chunkPos, world.seed - 1, biomeManager.temperatureNoise, voxelData.size.x, voxelData.size.y, voxelData.size.z);
        float[] humidityNoiseMap = NoiseCore.NoiseMap(chunkPos, world.seed + 1, biomeManager.humidityNoise, voxelData.size.x, voxelData.size.y, voxelData.size.z);

        #endregion

        // cycles through all the voxel data
        for (int x = 0; x < voxelData.size.x; x ++) {
            for (int y = 0; y < voxelData.size.y; y ++) {
                for (int z = 0; z < voxelData.size.z; z ++) {
                    // gets an index
                    int3 indexPos = new int3(x, y, z);
                    int index = IndexUtilities.Index3D(indexPos, voxelData.size.x, voxelData.size.y, voxelData.size.y);

                    // samples all the noise maps at the current index
                    float shapeNoiseSample = shapeNoiseMap[NoiseCore.IndexNoiseMap(indexPos, shapeNoiseMap.Last(), voxelData.size.x, voxelData.size.y, voxelData.size.z)];
                    float shapeNoise2Sample = shapeNoiseMap2[NoiseCore.IndexNoiseMap(indexPos, shapeNoiseMap2.Last(), voxelData.size.x, voxelData.size.y, voxelData.size.z)];
                    float overlayNoiseSample = overlayNoiseMap[NoiseCore.IndexNoiseMap(indexPos, overlayNoiseMap.Last(), voxelData.size.x, voxelData.size.y, voxelData.size.z)];
                    float temperatureNoiseSample = temperatureNoiseMap[NoiseCore.IndexNoiseMap(indexPos, temperatureNoiseMap.Last(), voxelData.size.x, voxelData.size.y, voxelData.size.z)];
                    float humidityNoiseSample = humidityNoiseMap[NoiseCore.IndexNoiseMap(indexPos, humidityNoiseMap.Last(), voxelData.size.x, voxelData.size.y, voxelData.size.z)];

                    // checks wether we are on the top or bottom of the chunk
                    bool onTopOrBottomOfChunk = y == 0 || y == voxelData.size.y - 1;

                    float value;

                    // patches up any holes if out of range of the chunk
                    if (!world.infYAxis && onTopOrBottomOfChunk) {
                        value = y == 0 ? 0f : 1f;
                    }
                    else {
                        // uses the noise samples to rise the terrain in a landscape formation
                        value = y + chunkPos.y + math.EPSILON; 
                        value /= (shapeNoiseSample * shapeNoise2Sample);
                        value *= overlayNoiseSample;

                        value = math.clamp(value, 0f, 1f);
                    }

                    // finds the closest biome based on the temperature and humidty samples
                    Biome biome = biomeManager.FindClosestBiome(temperatureNoiseSample, humidityNoiseSample, out biomeIndex);

                    // calculates the id based on the biome
                    VoxelId id = CalculateID(biome, biomeIndex, y);

                    // sets all the data
                    voxelData.data[index] = value;
                    voxelData.ids[index] = id;
                }
            }
        }

        return voxelData;
    }

    // a method that calculates the voxel id based on a few factors
    private static VoxelId CalculateID(Biome biome, int biomeIndex, int y) {
        // initialises the voxel id
        VoxelId voxelId = new VoxelId();

        // sets the biome
        voxelId.biome = biomeIndex;

        // for all the textures inside the biome textures
        foreach (BiomeTex biomeTex in biome.biomeTexes) {
            // finds the correct type based on the y value provided
            if (y < biomeTex.y) {
                voxelId.type = biomeTex.texIndex;
                break;
            }
        }

        return voxelId;
    }
}