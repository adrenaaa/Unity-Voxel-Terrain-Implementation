using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;

// allows the terrain to be terraformed
public class VoxelTerraformer : MonoBehaviour {
    [SerializeField] private World world;
    [Space(5f)]
    [SerializeField] private int range;
    [SerializeField] private float weight;
    [Space(5f)]
    [SerializeField] private bool canHold = true;
    [SerializeField, Range(0f, 1f)] private float smoothing = 0.6f;
    [Space(5f)]
    [SerializeField] private LayerMask terrainLayer;

    private RaycastHit terrainHit;

    private bool terraforming;

    private void Update() {
        if (!terraforming) {
            // if canHold is true, we can hold LMB or RMB to make snake formations in the terrain
            // however if its false then we just create balls
            if (canHold) {
                if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1)) {
                    DetectTerrain(Input.GetKey(KeyCode.Mouse1));
                }
            }
            else {
                if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1)) {
                    DetectTerrain(Input.GetKeyDown(KeyCode.Mouse1));
                }
            }
        }
    }

    private void DetectTerrain(bool remove) {
        // raycasts to the terrain to check where we want to terraform
        if (Physics.Raycast(transform.position, transform.forward, out terrainHit, 50f, terrainLayer)) {
            float3 point = terrainHit.point - transform.forward * 0.05f * ((remove) ? -1f : 1f);

            ModifyTerrain(remove, point);
        }
    }

    // the brains of modifying the terrain
    private async void ModifyTerrain(bool remove, float3 point) {
        terraforming = true;

        // the point we get is offset due to the centralisation of the vertices
        // so we undo this
        point -= World.centraliser;

        // round the point
        int3 roundedPoint = new int3 (
            (int) math.floor(point.x),
            (int) math.floor(point.y),
            (int) math.floor(point.z)
        );

        // keeps track of all the chunks that have been modified
        List<Chunk> chunksToUpdate = new List<Chunk>();

        // multiplies the weight depending of if we are removing the terrain or not
        int removeMult = (remove) ? 1 : -1;

        // loops based on the range given
        for (int x = -range; x < range; x ++) {
            for (int y = -range; y < range; y ++) {
                for (int z = -range; z < range; z ++) {
                    // gets the sample point based on the local position of the range
                    int3 offset = new int3(x, y, z);
                    int3 samplePoint = roundedPoint + offset;

                    float dstMult = 1f;

                    // if smoothing is 0, we create cubes
                    // however if it is 1, we create spheres
                    // 0.5 is in between
                    if (smoothing > 0f) {
                        int3 difference = (samplePoint - roundedPoint);
                        float dst = math.length(difference); // the fancy way of saying difference.magnitude

                        // if the distance between the sample point and original 
                        // point is out of range we remove any possibility of modifying
                        // that part of the terrain
                        if (dst >= range) {
                            continue;
                        }

                        // smoothsteps the distance mult based on the smoothing value and distance
                        dstMult = math.smoothstep(1f, 1f - smoothing, dst / range);
                    }

                    // gets all the neighbouring chunk positions
                    int3[] chunkPositions = world.chunkStorage.GetClosestChunkPositionsAtPoint(samplePoint, world.infYAxis);

                    // for all neighbouring chunk positions
                    foreach (int3 chunkPos in chunkPositions) {
                        // if the chunk exists at that position
                        if (world.chunkStorage.chunks.TryGetValue(chunkPos, out Chunk chunk)) {
                            // we take away chunkPos from the samplePoint so the point is localised to the chunk we are editing
                            int index = IndexUtilities.Index3D(samplePoint - chunkPos, chunk.voxelData.size.x, chunk.voxelData.size.y, chunk.voxelData.size.z);

                            // makes sure that the position gives is not out of range
                            if (index == -1 || samplePoint.y == 0 || samplePoint.y == chunk.voxelData.size.y - 1) {
                                continue;
                            }

                            // gets the sample from the storage based on the position
                            float sample = world.voxelDataStorage.allVoxelData[chunkPos].data[index];

                            // calculates the amount we add based on a multitude of factors
                            float add = (canHold) ? weight * dstMult * removeMult * Time.deltaTime : removeMult * dstMult;

                            // adds it to the sample
                            sample += add; 

                            // clamps the sample
                            sample = math.clamp(sample, 0f, 1f);

                            // reassigns the new sample to the storage data
                            world.voxelDataStorage.allVoxelData[chunkPos].data[index] = sample;

                            // checks if the chunk is queued for an update so we dont
                            // update a chunk multiple times accidentally
                            if (!chunk.isQueuedForUpdate) {
                                // adds the chunk to be a chunk to update
                                chunksToUpdate.Add(chunk);
                                chunk.isQueuedForUpdate = true;
                            }
                        }
                    }
                }
            }
        }

        // regenerates the chunk
        foreach (Chunk chunk in chunksToUpdate) {
            chunk.RegenerateMesh();

            while (chunk.waitingForMesh) {
                // since the chunk generation process is threaded,
                // we must wait until it is finished before continuing
                // so we dont edit a chunk twice while its already being edited
                // conveniently preventing unity from having a cardiac arrest
                await Task.Delay(1);
            }
        }

        foreach (Chunk chunk in chunksToUpdate) {
            // applies all the chunks meshes at the same time so we dont see holes
            // while modifying the terrain
            chunk.Apply();
            chunk.isQueuedForUpdate = false;
        }

        // stops terraforming
        terraforming = false;
    }
}