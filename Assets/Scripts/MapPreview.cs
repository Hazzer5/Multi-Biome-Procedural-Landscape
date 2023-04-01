using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public enum DrawMode {NoiseTexture, NoiseMesh}

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;

    [Header("Editor Files")]
    public MeshFilter mesh;
    private GameObject meshObj;
    public Renderer textureRender;
    private GameObject textureObj;

    public DrawMode mode;


    public void DrawInEditor() {
        meshObj = mesh.gameObject;
        textureObj = textureRender.gameObject;

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.mapChunkSize, meshSettings.mapChunkSize, heightMapSettings, new Vector2(0, 0));

        

        switch (mode) {
            case DrawMode.NoiseTexture:
            ApplyTexture(heightMap);
            meshObj.SetActive(false);
            textureObj.SetActive(true);
            break;
            case DrawMode.NoiseMesh:
            ApplyMesh(heightMap);
            textureObj.SetActive(false);
            meshObj.SetActive(true);
            break;
        }
    }


    void ApplyTexture(HeightMap heightMap) {
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
        textureObj.transform.localScale = new Vector3(width * 5.0f, 1, height * 5.0f);
    }

    void ApplyMesh(HeightMap heightMap) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap, meshSettings, 0);
        mesh.sharedMesh = meshData.CreateMesh();
    }
}
