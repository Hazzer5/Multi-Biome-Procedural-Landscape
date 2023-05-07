using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinusTerrainGenerator : MonoBehaviour
{
    [Header("World Settings")]
    public MeshSettings meshSettings;
    public BiomeNoiseSettings biomeNoiseSettings;
    public Material material;


    [Header("Viewer Settings")]
    public Transform viewer;
    public float maxViewDist;
    const float viewerMoveForChunkUpdate = 25f;
    const float sqrViewerMoveForChunkUpdate = viewerMoveForChunkUpdate * viewerMoveForChunkUpdate;

    Vector2 viewerPosition;
    Vector2 oldViewerPosition;


    float meshWorldSize;
    int chunkVisibleInViewDist;

    Dictionary<Vector2, TerrainChunk> chunkDictonary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleChunks = new List<TerrainChunk>();



    // Start is called before the first frame update
    void Start()
    {
        biomeNoiseSettings.CreateTextureArray();
        meshWorldSize = meshSettings.meshWorldSize;
        chunkVisibleInViewDist = Mathf.RoundToInt(maxViewDist / meshWorldSize);
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = 0; yOffset <= chunkVisibleInViewDist; yOffset++) {
            for (int xOffset = 0; xOffset <= chunkVisibleInViewDist; xOffset++) {
                Vector2 chunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                TerrainChunk newChunk = new TerrainChunk(chunkCoord, biomeNoiseSettings, meshSettings, transform, material, viewer, maxViewDist);
                chunkDictonary.Add(chunkCoord, newChunk);
                newChunk.onVisibiltyChange += OnChunkVissibiltyChange;

                newChunk.Load();

                if(yOffset==0 && xOffset==0) continue;

                chunkCoord = new Vector2(currentChunkCoordX - xOffset, currentChunkCoordY - yOffset);

                newChunk = new TerrainChunk(chunkCoord, biomeNoiseSettings, meshSettings, transform, material, viewer, maxViewDist);
                chunkDictonary.Add(chunkCoord, newChunk);
                newChunk.onVisibiltyChange += OnChunkVissibiltyChange;

                newChunk.Load();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if((oldViewerPosition - viewerPosition).sqrMagnitude > sqrViewerMoveForChunkUpdate) {
            oldViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks() {
        HashSet<Vector2> updatedChunkCoords = new HashSet<Vector2>();

        for (int i = visibleChunks.Count - 1; i >= 0; i--)
        {
            updatedChunkCoords.Add(visibleChunks[i].coord);
            visibleChunks[i].UpdateChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunkVisibleInViewDist; yOffset <= chunkVisibleInViewDist; yOffset++) {
            for (int xOffset = -chunkVisibleInViewDist; xOffset <= chunkVisibleInViewDist; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if(!updatedChunkCoords.Contains(viewedChunkCoord)) {
                    if(chunkDictonary.ContainsKey(viewedChunkCoord)) {
                        chunkDictonary[viewedChunkCoord].UpdateChunk();
                        if(chunkDictonary[viewedChunkCoord].IsVisible()) {
                            visibleChunks.Add(chunkDictonary[viewedChunkCoord]);
                        }
                    } else {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, biomeNoiseSettings, meshSettings, transform, material, viewer, maxViewDist);
                        chunkDictonary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibiltyChange += OnChunkVissibiltyChange;

                        newChunk.Load();
                    }
                }
            }
        }
    }

    void OnChunkVissibiltyChange(TerrainChunk chunk, bool isVisible) {
        if(isVisible) {
            visibleChunks.Add(chunk);
        } else {
            visibleChunks.Remove(chunk);
        }
    }
}
