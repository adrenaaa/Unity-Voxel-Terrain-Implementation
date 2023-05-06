using Unity.Mathematics;

// like the cube struct, this stores all our vertices
// just a float3 array
public struct VertexList {
    float3 vert0;
    float3 vert1;
    float3 vert2;
    float3 vert3;
    float3 vert4;
    float3 vert5;
    float3 vert6;
    float3 vert7;
    float3 vert8;
    float3 vert9;
    float3 vert10;
    float3 vert11;

    public void SetVertex(int index, float3 vert) {
        switch (index) {
            case 0:
                vert0 = vert;
            break;

            case 1:
                vert1 = vert;
            break;

            case 2:
                vert2 = vert;
            break;

            case 3:
                vert3 = vert;
            break;

            case 4:
                vert4 = vert;
            break;

            case 5:
                vert5 = vert;
            break;

            case 6:
                vert6 = vert;
            break;

            case 7:
                vert7 = vert;
            break;

            case 8:
                vert8 = vert;
            break;

            case 9:
                vert9 = vert;
            break;

            case 10:
                vert10 = vert;
            break;

            case 11:
                vert11 = vert;
            break;
        }
    }

    public float3 GetVertex(int index) {
        switch (index) {
            case 0: return vert0;
            case 1: return vert1;
            case 2: return vert2;
            case 3: return vert3;
            case 4: return vert4;
            case 5: return vert5;
            case 6: return vert6;
            case 7: return vert7;
            case 8: return vert8;
            case 9: return vert9;
            case 10: return vert10;
            case 11: return vert11;

            default: return -new float3(-1f, -1f, -1f);
        }
    }
}