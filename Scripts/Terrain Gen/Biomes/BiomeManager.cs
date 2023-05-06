using UnityEngine;

public class BiomeManager : MonoBehaviour {
    [Header("World")]
    [SerializeField] private World world;

    [Header("Noise")]
    public NoiseSettings temperatureNoise;
    public NoiseSettings humidityNoise;

    [Header("Biomes")]
    [SerializeField] private Biome[] biomes;

    private Texture2DArray topTexs, sideTexs;
    private Texture2DArray topNormals, sideNormals;

    private void Awake() {
        InitTextureArrays();
    }

    // creates texture arrays with all texture types
    private void InitTextureArrays() {
        int length = world.textureManager.worldTextures.Length;

        topTexs = new Texture2DArray(TextureManager.texSize, TextureManager.texSize, length, TextureFormat.RGBA32, true);
        sideTexs = new Texture2DArray(TextureManager.texSize, TextureManager.texSize, length, TextureFormat.RGBA32, true);
        topNormals = new Texture2DArray(TextureManager.texSize, TextureManager.texSize, length, TextureFormat.RGBA32, true);
        sideNormals = new Texture2DArray(TextureManager.texSize, TextureManager.texSize, length, TextureFormat.RGBA32, true);

        for (int i = 0; i < length; i ++) {
            WorldTexture worldTexture = world.textureManager.worldTextures[i];

            topTexs.SetPixels(worldTexture.topTex.GetPixels(), i);
            sideTexs.SetPixels(worldTexture.sideTex.GetPixels(), i);

            topNormals.SetPixels(worldTexture.topNormal.GetPixels(), i);
            sideNormals.SetPixels(worldTexture.sideNormal.GetPixels(), i);
        }

        topTexs.Apply();
        sideTexs.Apply();

        topNormals.Apply();
        sideNormals.Apply();

        world.mat.SetTexture("_Top_Texs", topTexs);
        world.mat.SetTexture("_Side_Texs", sideTexs);

        world.mat.SetTexture("_Top_Normals", topNormals);
        world.mat.SetTexture("_Side_Normals", sideNormals);
    }

    // finds the closest biome with the given temperature and humidity
    public Biome FindClosestBiome(float temperature, float humidity, out int biomeIndex) {
        biomeIndex = 0;

        Biome closestBiome = biomes[0];
        Vector2 pos = new Vector2(temperature, humidity);

        float minDst = float.MaxValue;
        int index = 0;

        // for every biome
        foreach (Biome biome in biomes) {
            // create a local position
            Vector2 biomePos = new Vector2(biome.temperature, biome.humidity);

            // find the distance
            float dst = (pos - biomePos).magnitude;

            // if the distance is less than the last recorded one note it down as the closest biome
            if (dst < minDst) {
                closestBiome = biome;
                minDst = dst;

                biomeIndex = index;
            }

            index ++;
        }

        return closestBiome;
    }
}

[System.Serializable]
public struct Biome {
    [SerializeField] private string biomeName;

    [Header("Textures")]
    public BiomeTex[] biomeTexes;
    
    [Header("Requirements")]
    [Range(-1f, 1f)] public float temperature;
    [Range(-1f, 1f)] public float humidity;
}

[System.Serializable]
public struct BiomeTex {
    [Header("Biome Texture Settings")]
    public int texIndex;
    [Space(5f)]
    [Range(0, World.sizeY - 1)] public int y;
}