using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{
    public static DataMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter) {
          
        float[,] values = new float[width, height];
        int[,,] biomes = new int[width, height, 3];
        float[,,] biomeEdges = new float[width, height, 2];

        float halfWidth = width/2;
        float halfHeight = height/2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                values[x,y] = NoiseGenerator.GeneratePerlinValue(x - halfWidth + sampleCenter.x, y-halfHeight - sampleCenter.y, settings);
            }
        }

        return new DataMap(values, biomes, biomeEdges);
    }

    public static DataMap GenerateBiomeMap(int width, int height, BiomeNoiseSettings settings, Vector2 sampleCenter) {
          
        float[,] values = new float[width, height];
        int[,,] biomes = new int[width, height, 3];
        float[,,] biomeEdges = new float[width, height, 2];

        float halfWidth = width/2;
        float halfHeight = height/2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                VoroniResult result = NoiseGenerator.GenerateVoronoiValue(x - halfWidth + sampleCenter.x, y-halfHeight - sampleCenter.y, settings);
                values[x,y] = result.cell1 / (float)settings.numBiomes;
                biomes[x,y,0] = result.cell1;
                biomes[x,y,1] = result.cell2;
                biomes[x,y,2] = result.cell3;
                biomeEdges[x,y,0] = result.edge1;
                biomeEdges[x,y,1] = result.edge2;
            }
        }

        return new DataMap(values, biomes, biomeEdges);
    }

}

public struct DataMap
{
    public readonly float[,] values;
    public readonly int[,,] biomes;
    public readonly float[,,] biomeEdges;

    public DataMap(float[,] heightMap, int[,,] biomes, float[,,] biomeEdges){
        this.values = heightMap;
        this.biomes = biomes;
        this.biomeEdges = biomeEdges;
    }
}