using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class TerrainGrassRepair
{
    private static readonly string GrassPrefabPath =
        "Assets/KMT/Environment/Grass/PT_Grass_02_LOD0.prefab";
    private static readonly string PoppyPrefabPath =
        "Assets/KMT/Environment/Grass/PT_Poppy_02_LOD0.prefab";
    private static readonly string GrassMaterialPath =
        "Assets/Polytope Studio/Lowpoly_Environments/Sources/Materials/PT_Grass_Mat.mat";
    private static readonly string PoppyMaterialPath =
        "Assets/Polytope Studio/Lowpoly_Environments/Sources/Materials/PT_Poppy_mat.mat";
    private static readonly HashSet<string> PriorityTerrainGuids = new HashSet<string>
    {
        "6a31112daf6579a458dae356ce5bbefd", // Map 2
        "1bc793e6b895f7646ab7831f65de4159"  // Map 8
    };

    [MenuItem("Tools/Kyatt Village Repair/Restore Terrain Grass")]
    public static void RestoreTerrainGrass()
    {
        NormalizeGrassPrefabTransform(GrassPrefabPath);
        NormalizeGrassPrefabTransform(PoppyPrefabPath);
        EnableMaterialInstancing(GrassMaterialPath);
        EnableMaterialInstancing(PoppyMaterialPath);

        GameObject grassPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GrassPrefabPath);
        GameObject poppyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PoppyPrefabPath);

        if (grassPrefab == null || poppyPrefab == null)
        {
            Debug.LogError("TerrainGrassRepair could not load the grass prefabs.");
            return;
        }

        string[] terrainDataGuids = AssetDatabase.FindAssets("t:TerrainData");
        int repairedTerrainCount = 0;

        foreach (string guid in terrainDataGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
            if (terrainData == null)
                continue;

            List<DetailPrototype> prototypes = new List<DetailPrototype>(terrainData.detailPrototypes);
            EnsurePrototype(prototypes, grassPrefab, 1.8f);
            EnsurePrototype(prototypes, poppyPrefab, 3.5f);
            terrainData.detailPrototypes = prototypes.ToArray();

            EnsureDetailMap(terrainData, FindPrototypeIndex(prototypes, grassPrefab), 5, 4);
            EnsureDetailMap(terrainData, FindPrototypeIndex(prototypes, poppyPrefab), 11, 1);

            if (PriorityTerrainGuids.Contains(guid))
            {
                RepaintDetailMap(
                    terrainData,
                    FindPrototypeIndex(prototypes, grassPrefab),
                    5,
                    4);
                RepaintDetailMap(
                    terrainData,
                    FindPrototypeIndex(prototypes, poppyPrefab),
                    11,
                    1);
            }

            EditorUtility.SetDirty(terrainData);
            repairedTerrainCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Restored grass on {repairedTerrainCount} TerrainData assets.");
    }

    private static void NormalizeGrassPrefabTransform(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
            return;

        Transform transform = prefab.transform;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        EditorUtility.SetDirty(prefab);
    }

    private static void EnableMaterialInstancing(string path)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
            return;

        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
    }

    private static void EnsurePrototype(
        List<DetailPrototype> prototypes,
        GameObject prototypePrefab,
        float minWidth)
    {
        foreach (DetailPrototype prototype in prototypes)
        {
            if (prototype.prototype == prototypePrefab ||
                (prototype.prototype != null && prototype.prototype.name == prototypePrefab.name))
            {
                prototype.usePrototypeMesh = true;
                prototype.renderMode = DetailRenderMode.VertexLit;
                prototype.minWidth = minWidth;
                prototype.maxWidth = minWidth * 1.5f;
                prototype.minHeight = minWidth;
                prototype.maxHeight = minWidth * 1.5f;
                return;
            }
        }

        prototypes.Add(new DetailPrototype
        {
            prototype = prototypePrefab,
            usePrototypeMesh = true,
            renderMode = DetailRenderMode.VertexLit,
            minWidth = minWidth,
            maxWidth = minWidth * 1.5f,
            minHeight = minWidth,
            maxHeight = minWidth * 1.5f,
            noiseSpread = 0.15f,
            healthyColor = Color.white,
            dryColor = Color.white
        });
    }

    private static int FindPrototypeIndex(
        List<DetailPrototype> prototypes,
        GameObject prototypePrefab)
    {
        for (int i = 0; i < prototypes.Count; i++)
        {
            DetailPrototype prototype = prototypes[i];
            if (prototype.prototype == prototypePrefab ||
                (prototype.prototype != null && prototype.prototype.name == prototypePrefab.name))
            {
                return i;
            }
        }

        return -1;
    }

    private static void EnsureDetailMap(
        TerrainData terrainData,
        int layer,
        int spacing,
        int density)
    {
        if (layer < 0 || terrainData.detailWidth <= 0 || terrainData.detailHeight <= 0)
            return;

        try
        {
            int[,] existingMap = terrainData.GetDetailLayer(
                0,
                0,
                terrainData.detailWidth,
                terrainData.detailHeight,
                layer);

            foreach (int value in existingMap)
            {
                if (value > 0)
                    return;
            }
        }
        catch (System.Exception)
        {
        }

        int[,] layerMap = new int[terrainData.detailHeight, terrainData.detailWidth];

        for (int y = 0; y < terrainData.detailHeight; y++)
        {
            for (int x = 0; x < terrainData.detailWidth; x++)
            {
                int hash = unchecked(x * 73856093 ^ y * 19349663 ^ spacing * 83492791);
                if ((hash & 0x7fffffff) % spacing == 0)
                    layerMap[y, x] = density;
            }
        }

        terrainData.SetDetailLayer(0, 0, layer, layerMap);
    }

    private static void RepaintDetailMap(
        TerrainData terrainData,
        int layer,
        int spacing,
        int density)
    {
        if (layer < 0 || terrainData.detailWidth <= 0 || terrainData.detailHeight <= 0)
            return;

        int[,] layerMap = new int[terrainData.detailHeight, terrainData.detailWidth];

        for (int y = 0; y < terrainData.detailHeight; y++)
        {
            for (int x = 0; x < terrainData.detailWidth; x++)
            {
                int hash = unchecked(x * 73856093 ^ y * 19349663 ^ spacing * 83492791);
                if ((hash & 0x7fffffff) % spacing == 0)
                    layerMap[y, x] = density;
            }
        }

        terrainData.SetDetailLayer(0, 0, layer, layerMap);
    }
}
