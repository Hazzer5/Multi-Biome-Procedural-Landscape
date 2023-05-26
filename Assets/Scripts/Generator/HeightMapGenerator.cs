using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{
    public static DataMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter) {
        AnimationCurve heightCurve_threadSafe = new AnimationCurve (settings.heightCurve.keys);
          
        float[,] values = new float[width, height];
        int[,,] biomes = new int[width, height, 3];
        float[,,] biomeEdges = new float[width, height, 2];

        float halfWidth = width/2;
        float halfHeight = height/2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                values[x,y] = heightCurve_threadSafe.Evaluate(NoiseGenerator.GeneratePerlinValue(x - halfWidth + sampleCenter.x, y-halfHeight - sampleCenter.y, settings));
            }
        }

        return new DataMap(values, biomes, biomeEdges);
    }

    public static DataMap GenerateBiomeMap(int width, int height, BiomeNoiseSettings settings, Vector2 sampleCenter) {
          
        float[,] values = new float[width, height];
        int[,,] biomes = new int[width, height, 3];
        float[,,] biomeWeights = new float[width, height, 2];


        AnimationCurve[] threadSafeHeightCurves = new AnimationCurve[settings.numBiomes];
        for (int i = 0; i < settings.numBiomes; i++)
        {
            threadSafeHeightCurves[i] = new AnimationCurve(settings.biomes[i].heightMapSettings.heightCurve.keys);
        }



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
                //biomeWeights[x,y,0] = Mathf.Max((result.edge1 / settings.biomeBlendDist), 0.0f);
                //biomeWeights[x,y,1] = Mathf.Max(1 - (result.edge2 / settings.biomeBlendDist), 0.0f);
                biomeWeights[x,y,0] = result.edge1;
                biomeWeights[x,y,1] = result.edge2;

                float mainWeight = 1 - biomeWeights[x,y,0] - biomeWeights[x,y,1];
                values[x,y] = 0;
                values[x,y] += threadSafeHeightCurves[result.cell1].Evaluate(NoiseGenerator.GeneratePerlinValue(x - halfWidth + sampleCenter.x, y-halfHeight - sampleCenter.y, settings.biomes[result.cell1].heightMapSettings)) * mainWeight;
                if(biomeWeights[x,y,0] > 0.0f) {
                    values[x,y] += threadSafeHeightCurves[result.cell2].Evaluate(NoiseGenerator.GeneratePerlinValue(x - halfWidth + sampleCenter.x, y-halfHeight - sampleCenter.y, settings.biomes[result.cell2].heightMapSettings)) *  biomeWeights[x,y,0];
                }
                if(biomeWeights[x,y,1] > 0.0f) {
                    values[x,y] += threadSafeHeightCurves[result.cell3].Evaluate(NoiseGenerator.GeneratePerlinValue(x - halfWidth + sampleCenter.x, y-halfHeight - sampleCenter.y, settings.biomes[result.cell3].heightMapSettings)) *  biomeWeights[x,y,1];
                }
            }
        }

        //Debug.Log(biomeWeights[0,0,1]);

        return new DataMap(values, biomes, biomeWeights);
    }

}

public struct DataMap
{
    public readonly float[,] values;
    public readonly int[,,] biomes;
    public readonly float[,,] biomeWeights;

    public DataMap(float[,] heightMap, int[,,] biomes, float[,,] biomeWeights){
        this.values = heightMap;
        this.biomes = biomes;
        this.biomeWeights = biomeWeights;
    }
}