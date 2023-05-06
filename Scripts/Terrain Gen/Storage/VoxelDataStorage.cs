using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

// just contains a dictionary that stores all the voxel data
public class VoxelDataStorage : MonoBehaviour {
    public Dictionary<int3, AllVoxelData> allVoxelData = new Dictionary<int3, AllVoxelData>();
}

public struct AllVoxelData {
    public readonly float[] data;
    public readonly VoxelId[] ids;

    public AllVoxelData(float[] data, VoxelId[] ids) {
        this.data = data;
        this.ids = ids;
    }
}