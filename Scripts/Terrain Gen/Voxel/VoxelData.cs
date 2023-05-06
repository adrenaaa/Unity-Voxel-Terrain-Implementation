using Unity.Mathematics;
using Unity.Collections;

// the struct that contains all the voxel data and ids
// used for generation
public struct VoxelData {
    public NativeArray<float> data;
    public NativeArray<VoxelId> ids;

    public readonly int3 size;
    public readonly int3 chunkPos;
    public readonly int sizeAll;

    // constructor
    public VoxelData(int sizeX, int sizeY, int sizeZ, int3 chunkPos) {
        sizeAll = sizeX * sizeY * sizeZ;
        size = new int3(sizeX, sizeY, sizeZ);

        this.chunkPos = chunkPos;

        data = new NativeArray<float>(sizeAll, Allocator.Persistent);
        ids = new NativeArray<VoxelId>(sizeAll, Allocator.Persistent);
    }

    // takes a position and creates a cube based on the values in the data array
    public Cube GetCube(int3 pos, float isoLevel) {
        Cube cube = new Cube();

        for (int i = 0; i < 8; i ++) {
            int3 indexPos = pos + MeshingCore.corners[i];
            int index = IndexUtilities.Index3D(indexPos, size.x, size.y, size.z);

            cube.SetCorner(i, data[index]);
        }

        return cube;
    }

    // packages the data currently in this struct and sends it off to storage
    public void SendAllVoxelDataToStorage(VoxelDataStorage storage) {
        AllVoxelData allVoxelData = new AllVoxelData(data.ToArray(), ids.ToArray());
        storage.allVoxelData.Add(chunkPos, allVoxelData);

        data.Dispose();
        ids.Dispose();
    }

    // takes all the voxel data in storage and retrieves it
    public void RetrieveAllVoxelDataFromStorage(VoxelDataStorage storage) {
        AllVoxelData allVoxelData = storage.allVoxelData[chunkPos];

        data = new NativeArray<float>(sizeAll, Allocator.Persistent);
        ids = new NativeArray<VoxelId>(sizeAll, Allocator.Persistent);

        for (int i = 0; i < sizeAll; i ++) {
            data[i] = allVoxelData.data[i];
            ids[i] = allVoxelData.ids[i];
        }

        storage.allVoxelData.Remove(chunkPos);
    }
}