using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMesh : MonoBehaviour
{

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;

    // Start is called before the first frame update
    void Start()
    {
        DataMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.mapChunkSize, meshSettings.mapChunkSize, heightMapSettings, new Vector2(0, 0));

        MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap, meshSettings, 0);


        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = meshData.CreateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
