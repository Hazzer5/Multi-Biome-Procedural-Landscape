using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeNoiseSettings : ScriptableObject
{
    public float cellSize;
    public int seed;

    public HeightMapSettings[] biomeHeightMaps;

    public int numBiomes {
        get {
            return biomeHeightMaps.Length;
        }
    }
    
}
