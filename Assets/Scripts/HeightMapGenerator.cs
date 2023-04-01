using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter) {
          
        float[,] values = new float[width, height];
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        float halfWidth = width/2;
        float halfHeight = height/2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                values[x,y] = GenerateNoiseValue(x - halfWidth + sampleCenter.x, y-halfHeight - sampleCenter.y, settings);
                if (minValue < values[x,y]) maxValue = values[x,y];
                if (minValue > values[x,y]) minValue = values[x,y];
            }
        }

        return new HeightMap(values, minValue, maxValue);
    }

    static float GenerateNoiseValue(float xCoord, float yCoord, HeightMapSettings settings) {
        float amplitude = 1;
        float frequncy = 1;
        float value = 0;
        for (int i = 0; i < settings.octaves; i++)
        {
            float sampleX = (xCoord + settings.offsets[i].x) / settings.scale * frequncy;
            float sampleY = (yCoord + settings.offsets[i].y) / settings.scale * frequncy;
            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
            value += perlinValue * amplitude;
            amplitude *= settings.persistance;
            frequncy *= settings.lacunarity;
        }
        float normalizedHeight = (value + 1)/(settings.maxHeight/0.9f);
        return Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
    }
}

public struct HeightMap
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] heightMap, float minValue, float maxValue){
        this.values = heightMap;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}