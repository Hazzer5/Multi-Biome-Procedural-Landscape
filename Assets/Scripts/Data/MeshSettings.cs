using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : ScriptableObject
{
   public const int numSuportedLODs = 5;
   public const int numSuportedChunkSizes = 5;
   public static readonly int[] supportedChunkSizes = {48,96,168,216,240};
   public static readonly int[] detailCounts = {1,2,3,4,6,8,12,16};

   [Range(0, numSuportedChunkSizes-1)]
   public int chunkSizeIndex;

   public float meshScale = 2.5f;
   public float heightMultiplier = 100.0f;

   public int mapChunkSize {
    get {
        return supportedChunkSizes[chunkSizeIndex] + 1;
    }
   }

   public float meshWorldSize {
    get {
        return (mapChunkSize - 3) * meshScale;
    }
   }
}