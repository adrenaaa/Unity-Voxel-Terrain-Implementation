using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

// this is the job that unpacks the vertex data into vertices, normals, uvs and triangles
[BurstCompile]
public struct VertexDataUnpackJob : IJob {
    [NativeDisableParallelForRestriction] private readonly NativeList<VertexData> vertexDatas;

    [NativeDisableParallelForRestriction] private readonly NativeList<float3> vertices;
    [NativeDisableParallelForRestriction] private readonly NativeList<float2> uvs;
    [NativeDisableParallelForRestriction] private readonly NativeList<int> triangles;

    private readonly bool smoothShading;

    // constructor
    public VertexDataUnpackJob(NativeList<VertexData> vertexDatas, NativeList<float3> vertices, NativeList<float2> uvs, NativeList<int> triangles, bool smoothShading) {
        this.vertexDatas = vertexDatas;
        this.vertices = vertices;
        this.uvs = uvs;
        this.triangles = triangles;
        this.smoothShading = smoothShading;
    }

    public void Execute() {
        // a nativehashmap is just the native version of a dictionary
        // we use this to track vertices
        NativeHashMap<float3, int> vertexTracker = new NativeHashMap<float3, int>(vertexDatas.Length, Allocator.Temp);

        int triIndex = 0;

        // for every vertex data
        foreach (VertexData vertexData in vertexDatas) {
            // we get the id
            float3 id = vertexData.vertexId;

            // if we want smooth shading (removing duplicate vertices), we need to share vertex indices
            // technique stolen from sebastian lague
            if (vertexTracker.TryGetValue(id, out int sharedVertexIndex) && smoothShading) {
                triangles.Add(sharedVertexIndex);
            }
            else {
                if (smoothShading) {
                    vertexTracker.Add(id, triIndex);
                }
                
                // extract and add to all our lists from the vertex data array
                // World.centraliser just moves the vertices to be centralised at the origin
                // of the chunk instead of the bottom corner
                vertices.Add(vertexData.pos + World.centraliser); 
                uvs.Add(new int2(vertexData.voxelId.type, 0));
                triangles.Add(triIndex);

                triIndex ++;
            }
        }

        vertexTracker.Dispose();
    }
}