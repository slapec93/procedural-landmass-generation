﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdatableData {

    public const int numberOfSupportedLODs = 5;
    public const int numberOfSupportedChunkSizes = 9;
    public const int numberOfSupportedFlatShadedChunkSizes = 3;
    public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float meshScale = 2.5f;
    public bool useFlatShading;

    [Range(0, numberOfSupportedChunkSizes - 1)]
    public int chunkSizeIndex;

    [Range(0, numberOfSupportedFlatShadedChunkSizes - 1)]
    public int flatShadedChunkSizeIndex;

    // Number of vertices per line of mesh rendered at LOD = 0. Includes the 2 extra vertices that are excluded form final mesh, but used for calculating normals
    public int numberOfVerticesPerLine {
        get {
            return supportedChunkSizes[(useFlatShading) ? flatShadedChunkSizeIndex : chunkSizeIndex] + 1;
        }
    }

    public float meshWorldSize {
        get {
            return (numberOfVerticesPerLine - 3) * meshScale;
        }
    }
}
