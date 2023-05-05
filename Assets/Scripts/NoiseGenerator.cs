using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator
{
    public static float GeneratePerlinValue(float xCoord, float yCoord, HeightMapSettings settings) {
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

    public static VoroniResult GenerateVoronoiValue(float xCoord, float yCoord, BiomeNoiseSettings settings) {
        float scaledXCoord = xCoord / settings.cellSize;
        float scaledYCoord = yCoord / settings.cellSize;
        Vector2 position = new Vector2(scaledXCoord, scaledYCoord);

        int baseCellX = (int)Mathf.Floor(scaledXCoord);
        int baseCellY = (int)Mathf.Floor(scaledYCoord);
        
        float minDist = 10;

        Vector2 bestCell = new Vector2(baseCellX, baseCellY);
        Vector2 toClosestCell = new Vector2(0,0);
        int cell = 0;

        for(int cellX=baseCellX-1; cellX <= baseCellX+1; cellX++) {
            for(int cellY=baseCellY-1; cellY <= baseCellY+1; cellY++) {
                var rand = new System.Random(((cellX << 16) ^ cellY) ^ settings.seed);
                Vector2 cellRandPos = new Vector2(cellX + (float)rand.NextDouble(), cellY + (float)rand.NextDouble());
                float dist = Vector2.Distance(position, cellRandPos);
                if(minDist > dist) {
                    minDist = dist;
                    bestCell.x = cellX;
                    bestCell.y = cellY;
                    
                    toClosestCell = cellRandPos - position;
                    cell = rand.Next(settings.numBiomes);
                } 
            }
        }

        VoroniResult result = new VoroniResult();
        result.cell1 = cell;

        float minEdgeDistance = 10.0f;
        int cell2 = 0;
        float secondMinEdgeDistance = 10.0f;
        int cell3 = 0;        

        for(int cellX=baseCellX-1; cellX <= baseCellX+1; cellX++) {
            for(int cellY=baseCellY-1; cellY <= baseCellY+1; cellY++) {
                if(bestCell.x == cellX && bestCell.y == cellY) continue;

                var rand = new System.Random(((cellX << 16) ^ cellY) ^ settings.seed);
                Vector2 cellRandPos = new Vector2(cellX + (float)rand.NextDouble(), cellY + (float)rand.NextDouble());
                Vector2 toCell = cellRandPos - position;
                Vector2 toCenter = (toClosestCell + toCell) * 0.5f;
                Vector2 cellDiff = (toCell-toClosestCell).normalized;
                float edgeDist = Vector2.Dot(toCenter, cellDiff);

                if(edgeDist < minEdgeDistance) {
                    secondMinEdgeDistance = minEdgeDistance;
                    cell3 = cell2;

                    minEdgeDistance = edgeDist;
                    cell2 = rand.Next(settings.numBiomes);
                } else if(edgeDist < secondMinEdgeDistance) {
                    secondMinEdgeDistance = edgeDist;
                    cell3 = rand.Next(settings.numBiomes);
                }
            }
        }
        
        result.cell2 = cell2;
        result.cell3 = cell3;
        result.edge1 = minEdgeDistance * settings.cellSize;
        result.edge2 = secondMinEdgeDistance * settings.cellSize;

        return result;
    }
}

public struct VoroniResult {
    public int cell1;
    public int cell2;
    public int cell3;
    public float edge1;
    public float edge2;
}
