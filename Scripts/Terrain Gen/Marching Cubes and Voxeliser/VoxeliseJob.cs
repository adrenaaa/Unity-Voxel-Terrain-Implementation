using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

// creates voxel terrain
[BurstCompile]
public struct VoxeliseJob : IJob {
    [NativeDisableParallelForRestriction] private readonly NativeList<VertexData> vertexDatas;

    private readonly VoxelData voxelData;

    private readonly float isoLevel;

    // constructor
    public VoxeliseJob(NativeList<VertexData> vertexDatas, VoxelData voxelData, float isoLevel) {
        this.vertexDatas = vertexDatas;
        this.voxelData = voxelData;
        this.isoLevel = isoLevel;
    }

    public void Execute() {
        // cycles through all the voxel data
        for (int x = 0; x < voxelData.size.x; x ++) {
            for (int y = 0; y < voxelData.size.y; y ++) {
                for (int z = 0; z < voxelData.size.z; z ++) {
                    // creates a local position
                    int3 pos = new int3(x, y, z);

                    Voxelise(pos);
                }
            }
        }
    }

    private void Voxelise(int3 pos) {
        // calculates the index at the provided local position
        int index = IndexUtilities.Index3D(pos, voxelData.size.x, voxelData.size.y, voxelData.size.z);

        // samples the voxel data
        float sample = voxelData.data[index];

        // if the sample is greater than the iso level then its out of range
        // so skip
        if (sample > isoLevel) {
            return;
        }

        // gets the id
        VoxelId id = voxelData.ids[index];

        // loop 6 times because a cube has 6 faces
        for (int i = 0; i < 6; i ++) {
            // samples the index right next to the current one
            int3 samplePos = pos + MeshingCore.voxelNeighbourSampleDirections[i];
            int neighbourIndex = IndexUtilities.Index3D(samplePos, voxelData.size.x, voxelData.size.y, voxelData.size.z);

            // if the neighbour doesnt exis (out of range) then theres nothing there so skip
            if (neighbourIndex == -1) {
                continue;
            }
            
            // gets the neighbour sample
            float neighbourSample = voxelData.data[neighbourIndex];

            // if the neighbour sample is solid then we cant see the face so
            // cull this face
            if (neighbourSample <= isoLevel) {
                continue;
            }

            // same logic with the marching cubes row index
            int rowIndex = i * 6;

            // each face has 2 triangles and one triangle has 3 vertices and 2 times 3 is 6
            // so we loop 6 times
            for (int j = 0; j < 6; j += 3) {
                // gets the tri index
                int triIndex = rowIndex + j;

                // find the correct vertex positions using the tri index
                float3 
                vertA = pos + MeshingCore.corners[MeshingCore.voxelTriTable[triIndex]],
                vertB = pos + MeshingCore.corners[MeshingCore.voxelTriTable[triIndex + 1]],
                vertC = pos + MeshingCore.corners[MeshingCore.voxelTriTable[triIndex + 2]];

                // creates new vertex datas
                VertexData
                a = new VertexData(vertA, id),
                b = new VertexData(vertB, id),
                c = new VertexData(vertC, id);

                // adds the vertex datas
                vertexDatas.Add(a);
                vertexDatas.Add(b);
                vertexDatas.Add(c);
            }
        }
    }
}