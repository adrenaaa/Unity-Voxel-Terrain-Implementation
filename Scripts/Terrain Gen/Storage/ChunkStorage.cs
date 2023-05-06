using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;

// stores all the chunks generated
public class ChunkStorage : MonoBehaviour {
    public Dictionary<int3, Chunk> chunks = new Dictionary<int3, Chunk>();

    // takes a point and finds all the closest neighbouring chunks within 1 unit
    public int3[] GetClosestChunkPositionsAtPoint(float3 point, bool infYAxis) {
        // if we arent using y axis chunks we dont need to consider the up and down directions
        int length = !infYAxis ? 9 : 25;
        int3[] chunkPositions = new int3[length];

        // gets the chunk at this position
        chunkPositions[0] = GetClosestChunkPosAtPoint(point, infYAxis);

        for (int i = 1; i < length; i ++) {
            // offsets the point given
            float3 samplePoint = point + MeshingCore.directions[i - 1];

            // finds the closest chunk
            chunkPositions[i] = GetClosestChunkPosAtPoint(samplePoint, infYAxis);
        }

        // returns all the chunks but with distinct so we dont have any duplicates
        return chunkPositions.Distinct().ToArray();
    }

    public int3 GetClosestChunkPosAtPoint(float3 point, bool infYAxis) {
        float
        x = point.x,
        y = point.y,
        z = point.z;

        // if the point is exactly on the chunk border, the point will be considered that its
        // on a chunk that its not on which will cause some errors
        if (x == math.floor(x)) {
            x -= 0.1f;
        }

        if (y == math.floor(y)) {
            y -= 0.1f;
        }

        if (z == math.floor(z)) {
            z -= 0.1f;
        }

        // rounds the point to a chunk pos
        return new int3 (
            (int) math.floor(x / World.sizeX) * World.sizeX,
            (!infYAxis) ? 0 : (int) math.floor(y / World.sizeY) * World.sizeY,
            (int) math.floor(z / World.sizeZ) * World.sizeZ
        );
    }
}