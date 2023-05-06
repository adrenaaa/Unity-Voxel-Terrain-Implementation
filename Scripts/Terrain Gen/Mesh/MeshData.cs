using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class MeshData {
    // the final mesh calculated
    public Mesh finalMesh;

    private readonly World world;
    private readonly VoxelData voxelData;

    // constructor
    public MeshData(World world, VoxelData voxelData) {
        this.world = world;
        this.voxelData = voxelData;
    }

    // generates the mesh using voxel data
    public void Generate() {
        // initialises the vertex data
        NativeList<VertexData> vertexDatas = new NativeList<VertexData>(Allocator.TempJob);

        JobHandle jobHandle;

        // if we arent voxelising the mesh
        if (!world.voxelise) {
            // create a new marching cubes job
            MarchingCubesJob marchingCubesJob = new MarchingCubesJob(vertexDatas, world.isoLevel, voxelData);

            // run the job
            jobHandle = marchingCubesJob.Schedule();
        }
        else {
            // we are voxelising the mesh so create a voxelise job
            VoxeliseJob voxeliseJob = new VoxeliseJob(vertexDatas,voxelData, world.isoLevel);

            jobHandle = voxeliseJob.Schedule();
        }

        // wait for the job to complete
        jobHandle.Complete();

        // send the voxel data to storage in case it needs to be accessed and modified later
        voxelData.SendAllVoxelDataToStorage(world.voxelDataStorage);

        // constructs a mesh using the vertex data
        ConstructMesh(vertexDatas);
    }

    private void ConstructMesh(NativeList<VertexData> vertexDatas) {
        // initialises the final mesh
        finalMesh = new Mesh();

        // creates a list for the vertices, uvs and triangles
        NativeList<float3> vertices = new NativeList<float3>(Allocator.TempJob);
        NativeList<float2> uvs = new NativeList<float2>(Allocator.TempJob);
        NativeList<int> triangles = new NativeList<int>(Allocator.TempJob);

        // creates a vertexdataunpack job
        VertexDataUnpackJob vertexDataUnpackJob = new VertexDataUnpackJob(vertexDatas, vertices, uvs, triangles, world.shadeSmooth && !world.voxelise);

        // runs and completes the job
        JobHandle jobHandle = vertexDataUnpackJob.Schedule();
        jobHandle.Complete();

        // disposes the vertex data
        vertexDatas.Dispose();

        // gets the generic length
        int length = vertices.Length;

        // turns the vertices and uvs into readable vector3s and vector2s instead of float3s and float2s
        // this looks inefficient but we would have to use .ToArray() anyway and it does the same thing
        Vector3[] processedVertices = new Vector3[length];
        Vector2[] processedUvs = new Vector2[length];

        for (int i = 0; i < length; i ++) {
            // we can just assign it like this because unity does an automatic conversion for 
            // individual float3s
            processedVertices[i] = vertices[i];
            processedUvs[i] = uvs[i];
        }

        // sets all the data
        finalMesh.vertices = processedVertices;
        finalMesh.triangles = triangles.ToArray();
        finalMesh.uv = processedUvs;

        // disposes all the native lists
        vertices.Dispose();
        triangles.Dispose();
        uvs.Dispose();

        // recalculates the mesh normals
        finalMesh.RecalculateNormals();
    }
}