using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

// all the brains of the generation process
[DisallowMultipleComponent]
public class World : MonoBehaviour {
    [Header("Biomes")]
    public BiomeManager biomeManager;
    public TextureManager textureManager;

    [Header("Factories")]
    public ChunkDataFactory chunkDataFactory;

    [Header("Storages")]
    public ChunkStorage chunkStorage;
    public VoxelDataStorage voxelDataStorage;

    [Header("Generation")]
    [SerializeField] private Vector3Int renderDst = new Vector3Int(8, 5, 8);
    public bool infYAxis;
    [Space(5f)]
    [SerializeField] private Transform target;

    [Header("Water")]
    public GameObject waterPrefab;
    public float waterLevel = 7.75f;
    public bool useWater = true;

    [Header("Mesh")]
    public Material mat;
    [Space(5f)]
    public bool shadeSmooth = true;
    public bool voxelise;
    [Space(5f)]
    [Range(0f, 1f)] public float isoLevel = 0.5f;

    [Header("Noise")]
    public int seed;
    [SerializeField] private bool randSeed = true;
    [Space(5f)]
    public NoiseSettings shapeNoise;
    public NoiseSettings shapeNoise2;
    public NoiseSettings overlayNoise;

    // chunk sizes
    public const int 
    sizeX = 12, 
    sizeY = 80, 
    sizeZ = 12;

    // half chunk sizes
    public const float 
    halfSizeX = sizeX * 0.5f, 
    halfSizeY = sizeY * 0.5f, 
    halfSizeZ = sizeZ * 0.5f;

    // the centraliser
    public static readonly float3 centraliser = -new float3(halfSizeX, halfSizeY, halfSizeZ);

    private Vector3Int targetPos, oldTargetPos;

    private Bounds targetBounds;
    private Queue<Chunk> chunksActiveInLastUpdate = new Queue<Chunk>();

    // checks wether the target pos is different to the old target pos
    // and updates the chunks accordingly
    private bool CanUpdateChunks() {
        return targetPos != oldTargetPos || Time.time == 0f;
    }

    private void Start() {
        // randomises the seed if needed
        if (randSeed) {
            RandomiseSeed();
        }
    }

    private void Update() {
        // sets the target pos
        // we do it this way so when we update the chunk positions
        // we can just directly add the target pos

        // also prevents generating chunks when nothing will change
        // because the position difference is too small so it saves performance
        targetPos = new Vector3Int (
            (int) math.round(target.position.x / sizeX) * sizeX,
            (!infYAxis) ? 0 : (int) math.round(target.position.y / sizeY) * sizeY,
            (int) math.round(target.position.z / sizeZ) * sizeZ
        );

        // if we can update the chunks then do it
        if (CanUpdateChunks()) {
            GenerateChunks();
        }
    }

    private void LateUpdate() {
        // sets the oldTargetPos to targetPos in late update 
        // so we can detect changes
        oldTargetPos = targetPos;
    }

    private void RandomiseSeed() {
        // min value is added one incase of the rare chance that we land on
        // the absolute min value and we dont overflow the compiler
        // when we take away one when calculating the humidity noise map

        // we dont do this on the max value becuase unitys random.range
        // makes it impossible to go on the top value, always one behind it
        // when dealing with integers
        seed = UnityEngine.Random.Range(int.MinValue + 1, int.MaxValue);
    }

    private void GenerateChunks() {
        // disables all chunks active last update incase they arent available to be iterated on
        // this generation iteration
        for (int i = 0; i < chunksActiveInLastUpdate.Count; i ++) {
            chunksActiveInLastUpdate.Dequeue().SetActive(false);
        }

        // gets the bound size
        Vector3 targetBoundsSize = new Vector3 (
            sizeX * (renderDst.x * renderDst.x), 
            sizeY * (renderDst.y * renderDst.y), 
            sizeZ * (renderDst.z * renderDst.z)
        );

        // creates a new bounds
        targetBounds = new Bounds(targetPos, targetBoundsSize);

        // cycles based on the render distance
        for (int x = -renderDst.x; x < renderDst.x; x ++) {
            for (int y = ((infYAxis) ? -renderDst.y : 0); y < ((infYAxis) ? renderDst.y : 1); y ++) {
                for (int z = -renderDst.z; z < renderDst.z; z ++) {
                    // calculates the chunk position
                    int3 pos = new int3(x * sizeX, y * sizeY, z * sizeZ) + new int3(targetPos.x, targetPos.y, targetPos.z);

                    if (!infYAxis) {
                        pos.y = 0;
                    }

                    // if a chunk has not been generated at this position
                    // generate it
                    if (!chunkStorage.chunks.ContainsKey(pos)) {
                        Chunk chunk = new Chunk(pos, this);
                        chunkStorage.chunks.Add(pos, chunk);
                    }

                    // validate the chunk at this position
                    ValidateChunk(pos);
                }
            }
        }
    }

    private void ValidateChunk(int3 pos) {
        // check if the chunk is in range of the bounds
        bool inRange = targetBounds.Contains(new Vector3Int(pos.x, pos.y, pos.z));

        // gets the chunk from chunk storage
        Chunk chunk = chunkStorage.chunks[pos];

        // sets it active based on the inRange variable
        chunk.SetActive(inRange);

        // if its in range then it is active therefore
        // it was active in the last update and needs to be
        // enqueued
        if (inRange) {
            chunksActiveInLastUpdate.Enqueue(chunk);
        }
    }
}