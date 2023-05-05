using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    public event System.Action<TerrainChunk, bool> onVisibiltyChange;
    public Vector2 coord; //chunk coordinate of this chunk

    GameObject meshObject;
    Vector2 sampleCenter; //the coordinates to use for generation
    Bounds bounds;


    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    DataMap heightMap;
    bool mapDataRecived;
    bool meshDataRecived;

    BiomeNoiseSettings biomeNoiseSettings;
    MeshSettings meshSettings;
    float vieableDist;

    Transform viewer;

    public TerrainChunk(Vector2 coord, BiomeNoiseSettings biomeNoiseSettings, MeshSettings meshSettings, Transform parent, Material material, Transform viewer, float vieableDist) {
        this.coord = coord;
        this.biomeNoiseSettings = biomeNoiseSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;
        this.vieableDist = vieableDist;

        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);
        sampleCenter = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        
        meshObject = new GameObject($"TerrainChunk {coord}");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
    }

    public void Load() {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateBiomeMap(meshSettings.mapChunkSize, meshSettings.mapChunkSize, biomeNoiseSettings, sampleCenter), OnMapDataReceived);
    }

    void OnMapDataReceived(object heightMapObject) {
        this.heightMap = (DataMap)heightMapObject;
        mapDataRecived = true;

        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap, meshSettings, 0), OnMeshDataReceived);
    }

    void OnMeshDataReceived(object meshDataObject) {
        meshFilter.sharedMesh = ((MeshData)meshDataObject).CreateMesh();

        meshDataRecived = true;

        UpdateChunk();
    }

    Vector2 viewerPosition {
        get {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }


    public void UpdateChunk() {
        if(!meshDataRecived) return;
        
        float viewerDist = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

        bool wasVisible = IsVisible();
        bool visible = viewerDist <= vieableDist;

        if (wasVisible != visible) {
            SetVisible(visible);
            if(onVisibiltyChange != null) {
                onVisibiltyChange (this, visible);
            }
        }
    }

    public void SetVisible(bool visible) {
        meshObject.SetActive(visible);
    }

    public bool IsVisible() {
        return meshObject.activeSelf;
    }
}
