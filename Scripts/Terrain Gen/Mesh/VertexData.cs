using Unity.Mathematics;

// contains all the data of a vertex
public struct VertexData {
    public readonly float3 pos;
    public readonly VoxelId voxelId;

    // distinguishes the vertex, if a vertex has the same position and id 
    // then it will have the same index
    public float3 vertexId {
        get {
            return pos + new int3(voxelId.type * 99999, 0, 0);
        }
    }

    // constructor
    public VertexData(float3 pos, VoxelId voxelId) {
        this.pos = pos;
        this.voxelId = voxelId;
    }
}

// voxel id
public struct VoxelId {
    public int type;
    public int biome;
}