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
        Texture2D normalSample = biomes[0].normal;
        
        Texture2DArray diffuseArray = new Texture2DArray(sample.width, sample.height, numBiomes, sample.format, true, false);
        Texture2DArray normalArray = new Texture2DArray(normalSample.width, normalSample.height, numBiomes, normalSample.format, true, true);
        diffuseArray.filterMode = FilterMode.Bilinear;
        diffuseArray.wrapMode = TextureWrapMode.Repeat;
        normalArray.filterMode = FilterMode.Bilinear;
        normalArray.wrapMode = TextureWrapMode.Repeat;

        for (int i = 0; i < numBiomes; i++)
        {
            Debug.Log(i);
            Graphics.CopyTexture(biomes[i].diffuse, 0, diffuseArray, i);
            Graphics.CopyTexture(biomes[i].normal, 0, normalArray, i);
        }
        diffuseArray.Apply();
        normalArray.Apply();

        material.SetTexture("_Diffuses", diffuseArray);
        material.SetTexture("_Normals", normalArray);
    }


}

[System.Serializable]
public class BiomeData {
    public HeightMapSettings heightMapSettings;
    public Texture2D diffuse;
    public Texture2D normal;
}
