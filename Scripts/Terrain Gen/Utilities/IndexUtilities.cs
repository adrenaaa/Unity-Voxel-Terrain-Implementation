using Unity.Mathematics;

// this class allows us to index arrays in a pseudo-multidimensional way
public static class IndexUtilities {
    public static int Index2D(int2 pos, int width, int height) {
        return Index2D(pos.x, pos.y, width, height);
    }

    public static int Index2D(int x, int y, int width, int height) {
        if (x >= width || x < 0) return -1;
        if (y >= height || y < 0) return -1;

        return x * width + y;
    }
    
    public static int Index3D(int3 pos, int width, int height, int length) {
        return Index3D(pos.x, pos.y, pos.z, width, height, length);
    }

    public static int Index3D(int x, int y, int z, int width, int height, int length) {
        if (x >= width || x < 0) return -1;
        if (y >= height || y < 0) return -1;
        if (z >= length || z < 0) return -1;

        return x * width * height + y * width + z;
    }
}