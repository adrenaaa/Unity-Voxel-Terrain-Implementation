using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System;
using Unity.Mathematics;

// this class is where chunks get their data from using threading
public class ChunkDataFactory : MonoBehaviour {
    [SerializeField] private World world;

    private Queue<ThreadInfo<VoxelData>> voxelDataThreadInfoQueue = new Queue<ThreadInfo<VoxelData>>();
    private Queue<ThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<ThreadInfo<MeshData>>();

    private void Update() {
        // using for loops so it holds up the game and we dont run the risk of falling off the map
        // due to chunks not generated in time with the player
        if (voxelDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < voxelDataThreadInfoQueue.Count; i ++) {
                // makes sure that the queue is not accessed twice
                lock (voxelDataThreadInfoQueue) {
                    ThreadInfo<VoxelData> voxelDataThreadInfo = voxelDataThreadInfoQueue.Dequeue();

                    // runs the callback with the generated data
                    voxelDataThreadInfo.callback(voxelDataThreadInfo.parameter);
                }
            }
        }

        // same as top for loop but with mesh data instead of voxel data
        if (meshDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i ++) {
                lock (meshDataThreadInfoQueue) {
                    ThreadInfo<MeshData> meshDataThreadInfo = meshDataThreadInfoQueue.Dequeue();

                    meshDataThreadInfo.callback(meshDataThreadInfo.parameter);
                }
            }
        }
    }

    // allows chunks to request voxel data
    public void RequestVoxelData(Action<VoxelData> callback, int3 chunkPos) {
        // creates a threadstart with a delegate so it can pass in arguments
        ThreadStart threadStart = delegate {
            VoxelDataThread(callback, chunkPos);
        };

        // creates a new thread
        Thread thread = new Thread(threadStart);

        // runs thread
        thread.Start();
    }

    private void VoxelDataThread(Action<VoxelData> callback, int3 chunkPos) {
        // creates a new voxel data
        VoxelData voxelData = VoxelDataGenerator.GenerateVoxelData(world, chunkPos);
        
        lock (voxelDataThreadInfoQueue) {
            ThreadInfo<VoxelData> voxelDataThreadInfo = new ThreadInfo<VoxelData>(callback, voxelData);

            voxelDataThreadInfoQueue.Enqueue(voxelDataThreadInfo);
        }
    }

    // same as requestvoxeldata method but with mesh data
    public void RequestMeshData(Action<MeshData> callback, VoxelData voxelData) {
        ThreadStart threadStart = delegate {
            MeshDataThread(callback, voxelData);
        };

        Thread thread = new Thread(threadStart);
        thread.Start();
    }

    private void MeshDataThread(Action<MeshData> callback, VoxelData voxelData) {
        // creating a constructor does not take long but we thread it anyway so it doesnt hold up any other chunks
        // from loading
        MeshData meshData = new MeshData(world, voxelData);

        lock (meshDataThreadInfoQueue) {
            ThreadInfo<MeshData> meshDataThreadInfo = new ThreadInfo<MeshData>(callback, meshData);

            meshDataThreadInfoQueue.Enqueue(meshDataThreadInfo);
        }
    }

    // generic struct we can pass data in to be threaded and transferred to the chunk
    private struct ThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;

        public ThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}