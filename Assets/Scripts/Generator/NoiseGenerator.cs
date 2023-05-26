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
        
        float minDistSqr = 100.0f;

        Vector2 bestCell = new Vector2(baseCellX, baseCellY);
        Vector2 toClosestCell = new Vector2(0,0);
        Vector2 cell1 = new Vector2(0,0);

        //find cell its in
        for(int cellX=baseCellX-1; cellX <= baseCellX+1; cellX++) {
            for(int cellY=baseCellY-1; cellY <= baseCellY+1; cellY++) {
                var rand = new System.Random(((cellX << 16) ^ cellY) ^ settings.seed);
                Vector2 cellRandPos = new Vector2(cellX + (float)rand.NextDouble(), cellY + (float)rand.NextDouble());
                float dist = Vector2.SqrMagnitude(cellRandPos - position);
                if(minDistSqr > dist) {
                    minDistSqr = dist;
                    bestCell.x = cellX;
                    bestCell.y = cellY;
                    cell1 = cellRandPos;
                    toClosestCell = cellRandPos - position;
                } 
            }
        }

        VoroniResult result = new VoroniResult();
        result.cell1 = new System.Random(cell1.GetHashCode() ^ settings.seed).Next(settings.numBiomes);

        float minEdgeDistance = 10.0f;
        float toCell2  = 100.0f;
        Vector2 cell2 = new Vector2(0,0);
        Vector2 secondBest = new Vector2(0,0);

        Vector2 toCell;
        Vector2 cellDiff;
        Vector2 toCenter;

        //find nearest edge
        for(int cellX=baseCellX-1; cellX <= baseCellX+1; cellX++) {
            for(int cellY=baseCellY-1; cellY <= baseCellY+1; cellY++) {
                if(bestCell.x == cellX && bestCell.y == cellY) continue;

                var rand = new System.Random(((cellX << 16) ^ cellY) ^ settings.seed);
                Vector2 cellRandPos = new Vector2(cellX + (float)rand.NextDouble(), cellY + (float)rand.NextDouble());
                toCell = cellRandPos - position;
                toCenter = (toClosestCell + toCell) * 0.5f;
                cellDiff = (toCell-toClosestCell).normalized;
                float edgeDist = Vector2.Dot(toCenter, cellDiff);

                if(edgeDist < minEdgeDistance) {
                    minEdgeDistance = edgeDist;
                    cell2 = cellRandPos;
                    toCell2 = toCell.sqrMagnitude;

                    secondBest.x = cellX;
                    secondBest.y = cellY;
                }
            }
        }

        float minVertexDistSqrd = 100.0f;
        Vector2 cell3 = new Vector2(0,0);
        float toCell3 = 100.0f;

        //find nearest vertex
        //eq found here https://en.wikipedia.org/wiki/Circumscribed_circle#Cartesian_coordinates_2
        for(int cellX=baseCellX-2; cellX <= baseCellX+2; cellX++) {
            for(int cellY=baseCellY-2; cellY <= baseCellY+2; cellY++) {
                if(bestCell.x == cellX && bestCell.y == cellY) continue;
                if(secondBest.x == cellX && secondBest.y == cellY) continue;

                var rand = new System.Random(((cellX << 16) ^ cellY) ^ settings.seed);
                Vector2 cellRandPos = new Vector2(cellX + (float)rand.NextDouble(), cellY + (float)rand.NextDouble());
                
                float aSqrd = cell1.x * cell1.x + cell1.y * cell1.y;
                float bSqrd = cell2.x * cell2.x + cell2.y * cell2.y;
                float cSqrd = cellRandPos.x * cellRandPos.x + cellRandPos.y * cellRandPos.y;

                float d = 2 *(cell1.x * (cell2.y - cellRandPos.y) + cell2.x * (cellRandPos.y - cell1.y) + cellRandPos.x * (cell1.y - cell2.y));

                float centerX = (aSqrd * (cell2.y - cellRandPos.y) + bSqrd * (cellRandPos.y - cell1.y) + cSqrd * (cell1.y - cell2.y)) / d;
                float centerY = (aSqrd * (cellRandPos.x - cell2.x) + bSqrd * (cell1.x - cellRandPos.x) + cSqrd * (cell2.x - cell1.x)) / d;

                Vector2 diff = new Vector2(centerX - position.x, centerY - position.y);

                float distSqr = diff.sqrMagnitude;
                if(distSqr < minVertexDistSqrd) {
                    minVertexDistSqrd = distSqr;
                    cell3 = cellRandPos;
                    toCell3 = (cellRandPos - position).sqrMagnitude;
                }
            }
        }


        //toCell2 -= minDistSqr;
        //toCell3 -= minDistSqr;


        result.cell2 = new System.Random(cell2.GetHashCode() ^ settings.seed).Next(settings.numBiomes);;
        result.cell3 = new System.Random(cell3.GetHashCode() ^ settings.seed).Next(settings.numBiomes);;

        float vertexDistWeight = 1.0f - Mathf.Sqrt(minVertexDistSqrd) * settings.cellSize / settings.biomeBlendDist * 2f;
        float edgeDistWeight = Mathf.Clamp(minEdgeDistance * settings.cellSize / settings.biomeBlendDist, 0.0f, 1.0f);
        result.edge1 = Mathf.Clamp((0.5f - edgeDistWeight) * Mathf.Lerp(1.0f, 0.660f, vertexDistWeight), 0.0f, 0.5f);// + Mathf.Lerp(0.0f, 0.33f, vertexDistWeight) ;

        //result.edge1 = toCell2 * settings.cellSize;
        
        result.edge2 = Mathf.Lerp(0f, 0.33f, vertexDistWeight);

        // toCell = cell3 - position;
        // toCenter = (toClosestCell + toCell) * 0.5f;
        // cellDiff = (toCell-toClosestCell).normalized;
        // result.edge2 =  (Vector2.Dot(toCenter, cellDiff) / Mathf.Sqrt(minVertexDistSqrd));// * settings.cellSize;

        // float secondaryWeights = result.edge1 + result.edge2;
        // float mainWeight = 1 - secondaryWeights * secondaryWeights;
        // float scalar = mainWeight + secondaryWeights;

        // result.edge1 /= scalar;
        // result.edge2 /= scalar;

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
