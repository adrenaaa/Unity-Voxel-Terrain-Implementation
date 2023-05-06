using UnityEngine;
using Unity.Mathematics;

// the class that contains all the information of a chunk
public class Chunk {
    private readonly World world;

    private readonly int3 chunkPos;

    private GameObject chunkObj;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public VoxelData voxelData;
    private MeshData meshData;

    public bool isQueuedForUpdate;
    public bool waitingForMesh;

    // constructor
    public Chunk(int3 chunkPos, World world) {
        this.chunkPos = chunkPos;
        this.world = world;

        // generates the chunk and water objects
        GenerateChunkObj();
        
        if (world.useWater) {
            GenerateWaterObj();
        }
        
        // requests the voxel data required to generate the mesh data and starts
        // the chunk generation process
        world.chunkDataFactory.RequestVoxelData(VoxelDataReceived, chunkPos);
    }

    private void GenerateChunkObj() {
        // creates a new gameobject
        chunkObj = new GameObject($"Chunk: {chunkPos.x}, {chunkPos.y}, {chunkPos.z}");

        // sets the correct position and parent
        chunkObj.transform.position = new Vector3(chunkPos.x, chunkPos.y, chunkPos.z);
        chunkObj.transform.SetParent(world.transform);

        // adds the required components
        meshRenderer = chunkObj.AddComponent<MeshRenderer>();
        meshFilter = chunkObj.AddComponent<MeshFilter>();
        meshCollider = chunkObj.AddComponent<MeshCollider>();

        // assigns the correct material
        meshRenderer.material = world.mat;
    }

    private void GenerateWaterObj() {
        // creates the position the water is to be spawned at
        Vector3 waterPos = chunkObj.transform.position;
        waterPos.y = World.centraliser.y + world.waterLevel;

        // instantiates the water prefab
        Transform water = MonoBehaviour.Instantiate(world.waterPrefab, waterPos, Quaternion.identity).transform;

        // the water mesh is a plane so we scale the water accordingly
        water.localScale = new Vector3(World.sizeX * 0.1f, 1f, World.sizeZ * 0.1f);

        // we set the water to be a child of the chunk object
        water.SetParent(chunkObj.transform);
    }

    private void VoxelDataReceived(VoxelData voxelData) {
        // when voxel data is received we start generating the mesh
        this.voxelData = voxelData;

        waitingForMesh = true;
        world.chunkDataFactory.RequestMeshData(MeshDataReceived, voxelData);
    }

    private void MeshDataReceived(MeshData meshData) {
        // generate the mesh
        meshData.Generate();
        this.meshData = meshData;

        waitingForMesh = false;

        // apply the mesh
        Apply();
    }

    // same thing as the above method but we dont apply the mesh immediately
    private void MeshDataReceivedNoAutoApply(MeshData meshData) {
        meshData.Generate();
        this.meshData = meshData;

        waitingForMesh = false;
    }

    public void Apply() {
        // applies the generated mesh to the mesh filter and mesh collider
        meshFilter.mesh = meshData.finalMesh;
        meshCollider.sharedMesh = meshData.finalMesh;
    }

    // regenerates the mesh by taking the stored data from
    // the voxel data storage and remaking the mesh using that
    public void RegenerateMesh(bool autoApply = false) {
        waitingForMesh = true;

        voxelData.RetrieveAllVoxelDataFromStorage(world.voxelDataStorage);
        
        world.chunkDataFactory.RequestMeshData((!autoApply) ? MeshDataReceivedNoAutoApply : MeshDataReceived, voxelData);
    }

    public void SetActive(bool active) {
        // sets the chunk object active depending on the argument
        chunkObj.SetActive(active);
    }
}