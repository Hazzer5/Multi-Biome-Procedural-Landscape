using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public enum DrawMode {NoiseTexture, NoiseMesh, Biome1Texture, Biome2Texture, BiomeEdgeTexture, SecondBiomeEdgeTexture, MainBiomeWeight}

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;

    public BiomeNoiseSettings biomeNoiseSettings;

    [Header("Editor Files")]
    public MeshFilter mesh;
    private GameObject meshObj;
    public Renderer textureRender;
    private GameObject textureObj;

    public DrawMode mode;


    public void DrawInEditor() {
        biomeNoiseSettings.Initialize();

        meshObj = mesh.gameObject;
        textureObj = textureRender.gameObject;

        DataMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.mapChunkSize, meshSettings.mapChunkSize, heightMapSettings, new Vector2(0, 0));
        DataMap biomeMap = HeightMapGenerator.GenerateBiomeMap(meshSettings.mapChunkSize, meshSettings.mapChunkSize, biomeNoiseSettings, new Vector2(0, 0));
        

        switch (mode) {
            case DrawMode.NoiseTexture:
            ApplyTexture(heightMap);
            meshObj.SetActive(false);
            textureObj.SetActive(true);
            break;
            case DrawMode.Biome1Texture:
            case DrawMode.Biome2Texture:
            case DrawMode.BiomeEdgeTexture:
            case DrawMode.SecondBiomeEdgeTexture:
            case DrawMode.MainBiomeWeight:
            ApplyBiomeTexture(biomeMap, mode);
            meshObj.SetActive(false);
            textureObj.SetActive(true);
            break;
            case DrawMode.NoiseMesh:
            ApplyMesh(biomeMap);
            textureObj.SetActive(false);
            meshObj.SetActive(true);
            break;
            
        }
    }


    void ApplyTexture(DataMap heightMap) {
        int width = heightMap.values.GetLength(0);
        int height = heightMap.values.GetLength(1);
        Texture2D text = new Texture2D(width, height);
        Color[] colourMap = new Color[width * height];
         for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                colourMap[y*width+x] = Color.Lerp(Color.black, Color.white, heightMap.values[x,y]);
            }
        }
        
        text.filterMode = FilterMode.Point;
        text.wrapMode = TextureWrapMode.Clamp;
        text.SetPixels(colourMap);
        text.Apply();

        textureRender.sharedMaterial.mainTexture = text;
        textureObj.transform.localScale = new Vector3(width * -0.1f, 1, height * -0.1f);
    }
    void ApplyBiomeTexture(DataMap heightMap, DrawMode type) {
        int width = heightMap.values.GetLength(0);
        int height = heightMap.values.GetLength(1);
        Texture2D text = new Texture2D(width, height);
        Color[] colourMap = new Color[width * height];
         for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = 0;
                switch(type) {
                    case DrawMode.Biome1Texture:
                    value = heightMap.biomes[x,y,0] / (biomeNoiseSettings.numBiomes - 1.0f);
                    break;
                    case DrawMode.Biome2Texture:
                    value = heightMap.biomes[x,y,1] / (biomeNoiseSettings.numBiomes - 1.0f);
                    break;
                    case DrawMode.BiomeEdgeTexture:
                    value = heightMap.biomeWeights[x,y,0];
                    break;
                    case DrawMode.SecondBiomeEdgeTexture:
                    value = (heightMap.biomeWeights[x,y,1]);
                    break;
                    case DrawMode.MainBiomeWeight:
                    value = 1 - heightMap.biomeWeights[x,y,0] - heightMap.biomeWeights[x,y,1];
                    break;
                }


                colourMap[y*width+x] = Color.Lerp(Color.black, Color.white, value);
            }
        }
        
        text.filterMode = FilterMode.Point;
        text.wrapMode = TextureWrapMode.Clamp;
        text.SetPixels(colourMap);
        text.Apply();

        textureRender.sharedMaterial.mainTexture = text;
        textureObj.transform.localScale = new Vector3(width * -0.1f, 1, height * 0.1f);
    }



    void ApplyMesh(DataMap heightMap) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap, meshSettings, 0);
        mesh.sharedMesh = meshData.CreateMesh();
    }
}
