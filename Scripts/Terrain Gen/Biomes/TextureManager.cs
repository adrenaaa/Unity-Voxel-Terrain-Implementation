using UnityEngine;

public class TextureManager : MonoBehaviour {
    // just stores all the textures that are used in the world
    public WorldTexture[] worldTextures;

    public const int texSize = 1024;
}

[System.Serializable]
public struct WorldTexture {
    [SerializeField] private string texLabel;

    [Header("Textures")]
    [Tooltip("1024x1024")] public Texture2D topTex;
    [Tooltip("1024x1024")] public Texture2D sideTex;
    
    [Header("Normals")]
    [Tooltip("1024x1024")] public Texture2D topNormal;
    [Tooltip("1024x1024")] public Texture2D sideNormal;
}