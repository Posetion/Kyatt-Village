using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FixTerrainTrees : MonoBehaviour
{
    [Header("Tree Vertical Offset")]
    [Tooltip("Positive values raise trees, negative values sink trees into the ground.")]
    [Range(-2f, 2f)]
    public float offsetY = -0.1f;

    [ContextMenu("Fix Terrain Trees Now")]
    public void FixTreesOnTerrain()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null || terrain.terrainData == null) return;

        TreePrototype[] prototypes = terrain.terrainData.treePrototypes;

        foreach (TreePrototype proto in prototypes)
        {
            if (proto.prefab == null) continue;

            MeshFilter mf = proto.prefab.GetComponentInChildren<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Mesh mesh = mf.sharedMesh;
                Mesh newMesh = Instantiate(mesh);
                Vector3[] newVerts = newMesh.vertices;

                // Apply the height/depth adjustment defined in the Inspector
                for (int i = 0; i < newVerts.Length; i++)
                {
                    newVerts[i].y += offsetY;
                }

                newMesh.vertices = newVerts;
                newMesh.RecalculateBounds();
                newMesh.RecalculateNormals();

#if UNITY_EDITOR
                // Save mesh asset to disk so Unity retains it after saving/reloading
                string path = "Assets/AdjustedTreeMesh_" + proto.prefab.name + ".asset";
                AssetDatabase.CreateAsset(newMesh, path);
                AssetDatabase.SaveAssets();
                newMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
#endif

                mf.sharedMesh = newMesh;
            }
        }

        terrain.Flush();
        Debug.Log($"Trees adjusted by {offsetY} units and saved to disk!");
    }
}