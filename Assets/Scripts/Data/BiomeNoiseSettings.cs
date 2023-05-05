using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeNoiseSettings : ScriptableObject
{
    public float cellSize;
    public int seed;

    public BiomeData[] biomes;
    public Material material;


    public int numBiomes {
        get {
            return biomes.Length;
        }
    }
    
    public void CreateTextureArray() {
        Texture2D sample = biomes[0].diffuse;
        
        Texture2DArray textureArray = new Texture2DArray(sample.width, sample.height, numBiomes, sample.format, true, false);
        textureArray.filterMode = FilterMode.Bilinear;
        textureArray.wrapMode = TextureWrapMode.Repeat;

        for (int i = 0; i < numBiomes; i++)
        {
            Debug.Log(i);
            Graphics.CopyTexture(biomes[i].diffuse, 0, textureArray, i);
        }
        textureArray.Apply();

        material.SetTexture("_Diffuses", textureArray);
    }


}

[System.Serializable]
public class BiomeData {
    public HeightMapSettings heightMapSettings;
    public Texture2D diffuse;
    public Texture2D normal;
}
