using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(HeightMap heightMap, MeshSettings meshSettings, int levelOfDetail) {
        int meshSimplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail*2;

        int borderedSize = heightMap.values.GetLength(0);
        int meshSize = borderedSize - 2;

        float topLeftX = (meshSize - 1) / -2f;
        float topLeftZ = (meshSize - 1) / 2f;

        int verticesPerLine = ((meshSize - 1) / meshSimplificationIncrement) + 1;

        MeshData meshData = new MeshData(verticesPerLine);

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;
        Debug.Log(borderedSize);
        for (int y = 0; y < borderedSize; y++)
        {
            for (int x = 0; x < borderedSize; x++)
            {
                bool isBorderVertex = (y==0 || y==borderedSize - 1 || x==0 || x==borderedSize - 1);
                if(isBorderVertex) {
                    vertexIndicesMap[x,y] = borderVertexIndex;
                    borderVertexIndex--;
                } else {
                    vertexIndicesMap[x,y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < borderedSize; y++)
        {
            for (int x = 0; x < borderedSize; x++)
            {
                int vertexIndex = vertexIndicesMap[x,y];
                Vector2 percent = new Vector2(x-1.0f / meshSize, y-1.0f / meshSize);
                float height = heightMap.values[x,y];
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSize, height * meshSettings.heightMultiplier, topLeftZ - percent.y * meshSize) * meshSettings.meshScale;

                meshData.AddVertex(vertexPosition, percent, vertexIndex);
                if (x < borderedSize - 1 && y < borderedSize - 1) {
                    int a = vertexIndicesMap[x,    y];
                    int b = vertexIndicesMap[x + 1,y];
                    int c = vertexIndicesMap[x,    y + 1];
                    int d = vertexIndicesMap[x + 1,y + 1];
                    meshData.AddTriangle(a,d,c);
                    meshData.AddTriangle(d,a,b);
                }
            }
        }
        meshData.CalculateNormals();
        return meshData;
    }
}

public class MeshData {
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Vector3[] bakedNormals;

    Vector3[] borderVertices;
    int[] borderTriangles;

    int triangleIndex;
    int borderTriangleIndex;

    public MeshData(int verticesPerLine) {
        int totalVertices = verticesPerLine * verticesPerLine;
        vertices = new Vector3[totalVertices];
        uvs = new Vector2[totalVertices];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        borderVertices = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticesPerLine];
    }
    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex){
        if(vertexIndex < 0) {
            borderVertices [-vertexIndex - 1] = vertexPosition;
        } else {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }
    public void AddTriangle(int a, int b, int c) {
        if(a < 0 || b < 0 || c < 0) {
            borderTriangles[borderTriangleIndex]     = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;
        } else {
            triangles[triangleIndex]     = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
    }

    public void CalculateNormals() {
        bakedNormals = new Vector3[vertices.Length];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int vertexIndexA = triangles[i];
            int vertexIndexB = triangles[i + 1];
            int vertexIndexC = triangles[i + 2];
            Vector3 normal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            bakedNormals[vertexIndexA] += normal;
            bakedNormals[vertexIndexB] += normal;
            bakedNormals[vertexIndexC] += normal;
        }

        for (int i = 0; i < borderTriangles.Length; i += 3)
        {
            int vertexIndexA = borderTriangles[i];
            int vertexIndexB = borderTriangles[i + 1];
            int vertexIndexC = borderTriangles[i + 2];
            Vector3 normal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if(vertexIndexA >= 0) bakedNormals[vertexIndexA] += normal;
            if(vertexIndexB >= 0) bakedNormals[vertexIndexB] += normal;
            if(vertexIndexC >= 0) bakedNormals[vertexIndexC] += normal;
        }
        for (int i = 0; i < bakedNormals.Length; i++)
        {
            bakedNormals[i].Normalize();
        }
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
        Vector3 a = (indexA < 0)?borderVertices[-indexA-1]:vertices[indexA];
        Vector3 b = (indexB < 0)?borderVertices[-indexB-1]:vertices[indexB];
        Vector3 c = (indexC < 0)?borderVertices[-indexC-1]:vertices[indexC];

        Vector3 ab = b - a;
        Vector3 ac = c - a;
        return Vector3.Cross(ab, ac).normalized;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = bakedNormals;
        return mesh;
    }
}