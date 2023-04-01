using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : ScriptableObject
{
    public int octaves = 3;
    public float scale = 50;

    [Range(0,1)]
    public float persistance = 0.5f;
    public float lacunarity = 2;

    public int seed;

    [SerializeField]private Vector2 fixedOffset;

    [HideInInspector]
    public Vector2[] offsets;

    [HideInInspector]
    public float maxHeight;

    void OnValidate() {
        octaves = Mathf.Max(octaves, 1);
        if (scale <= 0) scale = 10.0f;

        offsets = new Vector2[octaves];
        System.Random prng = new System.Random(seed);

        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);

        float amplitude = 1;
        maxHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next (-100000, 100000) + fixedOffset.x;
            float offsetY = prng.Next (-100000, 100000) - fixedOffset.y;
            offsets[i] = new Vector2 (offsetX, offsetY);
            maxHeight += amplitude;
            amplitude *= persistance;
        }
    }
    
}
