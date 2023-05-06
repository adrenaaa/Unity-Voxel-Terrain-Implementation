using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

// this is the struct that actually processes the marching cubes
[BurstCompile]
public struct MarchingCubesJob : IJob {
    [NativeDisableParallelForRestriction] private readonly NativeList<VertexData> vertexDatas;

    private readonly float isoLevel;
    private readonly VoxelData voxelData;

    // constructor
    public MarchingCubesJob(NativeList<VertexData> vertexDatas, float isoLevel, VoxelData voxelData) {
        this.vertexDatas = vertexDatas;
        this.isoLevel = isoLevel;
        this.voxelData = voxelData;
    }

    // executes on a separate thread
    public void Execute() {
        // minus 1 so when we create the cube it doesnt go out of range
        for (int x = 0; x < voxelData.size.x - 1; x ++) {
            for (int y = 0; y < voxelData.size.y - 1; y ++) {
                for (int z = 0; z < voxelData.size.z - 1; z ++) {
                    // creates a local position
                    int3 pos = new int3(x, y, z);

                    // marches at that position
                    March(pos);
                }
            }
        }
    }

    private void March(int3 pos) {
        // gets the cube at the position
        Cube cube = voxelData.GetCube(pos, isoLevel);

        // calculates the cube index using the cube and iso level
        int cubeIndex = MeshingCore.CalculateCubeIndex(cube, isoLevel);

        // the triangulation at this cube index is all -1 so this saves processing power
        if (cubeIndex == 0 || cubeIndex == 255) {
            return;
        }

        // creates vertex list
        VertexList vertexList = MeshingCore.CalculateVertexList(pos, cube, isoLevel, cubeIndex);

        // since there is 16 triangle indices and we arent using multidimensional arrays
        // we can do this simple multiplication to always default the cube index at the start
        // of a triangulation indices row
        int rowIndex = cubeIndex * 16;

        // gets the triangle id
        int index3D = IndexUtilities.Index3D(pos, voxelData.size.x, voxelData.size.y, voxelData.size.z);
        VoxelId id = voxelData.ids[index3D];

        // cycles through the tritable at the row index
        for (int i = 0; MeshingCore.triTable[rowIndex + i] != -1; i += 3) {
            // triangle index
            int triIndex = rowIndex + i;

            // vertices of one triangle
            float3 
            vertA = vertexList.GetVertex(MeshingCore.triTable[triIndex]),
            vertB = vertexList.GetVertex(MeshingCore.triTable[triIndex + 1]),
            vertC = vertexList.GetVertex(MeshingCore.triTable[triIndex + 2]);

            // converts the vertices of the triangle into vertex datas
            VertexData
            a = new VertexData(vertA,  id),
            b = new VertexData(vertB,  id),
            c = new VertexData(vertC,  id);

            // sends all the vertex data to the nativelist
            vertexDatas.Add(a);
            vertexDatas.Add(b);
            vertexDatas.Add(c);
        }
    }
}