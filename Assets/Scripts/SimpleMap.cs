using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMap : MonoBehaviour
{
    public int width;
    public int height;

    public HeightMapSettings heightMapSettings;
    // Start is called before the first frame update
    void Start()
    {
        Texture2D text = new Texture2D(width, height);
        Color[] colourMap = new Color[width * height];

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(width, height, heightMapSettings);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                colourMap[y*width+x] = Color.Lerp(Color.black, Color.white, heightMap.values[x,y]);
            }
        }

        Debug.Log(heightMap.values);

        text.filterMode = FilterMode.Point;
        text.wrapMode = TextureWrapMode.Clamp;
        text.SetPixels(colourMap);
        text.Apply();

        Renderer textureRender = GetComponent<Renderer>();
        textureRender.sharedMaterial.mainTexture = text;
        transform.localScale = new Vector3(width, 1, height);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
