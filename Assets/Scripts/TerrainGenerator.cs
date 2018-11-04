using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrtViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public Transform viewer;
    public Material mapMaterial;

    Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    float meshWorldSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    void Start() {
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

        UpdateVisibleChunks();
    }

    void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != viewerPositionOld) {
            foreach (TerrainChunk chunk in visibleTerrainChunks) {
                chunk.UpdateCollisionMesh();
            }
        }

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrtViewerMoveThresholdForChunkUpdate) {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks() {

        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--) {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if(alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) {
                    continue;
                }
                if(terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                } else {
                    TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial);
                    terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                    newChunk.onVisibilityChanged += OnTerrainVisibilityChanged;
                    newChunk.Load();
                }
            }
        }
    }

    void OnTerrainVisibilityChanged(TerrainChunk chunk, bool isVisible) {
        if (isVisible) {
            visibleTerrainChunks.Add(chunk);
        } else {
            visibleTerrainChunks.Remove(chunk);
        }
    }
}

[System.Serializable]
public struct LODInfo {
    [Range(0, MeshSettings.numberOfSupportedLODs - 1)]
    public int lod;
    public float visibleDistanceThreshold;

    public float sqrVisibleDistanceThreshold {
        get {
            return visibleDistanceThreshold * visibleDistanceThreshold;
        }
    }
}