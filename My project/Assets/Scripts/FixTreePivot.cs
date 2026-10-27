using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Terrain))]
public class FixTerrainTreeOffset : MonoBehaviour
{
    [Header("Tree Offset Settings")]
    [Tooltip("Target offset to lift the trees. For a Quad scaled to 5 on Y, this should be 2.5.")]
    [SerializeField] private float yOffset = 2.5f;

    [ContextMenu("Fix Terrain Trees Now")]
    public void FixTrees()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null || terrain.terrainData == null) return;

        TerrainData data = terrain.terrainData;
        TreePrototype[] prototypes = data.treePrototypes;

        if (prototypes.Length == 0)
        {
            Debug.LogWarning("No tree prototypes found on Terrain.");
            return;
        }

        // Fix the raw mesh geometry on prototypes using Quad meshes
        for (int i = 0; i < prototypes.Length; i++)
        {
            GameObject treePrefab = prototypes[i].prefab;
            if (treePrefab != null)
            {
                MeshFilter mf = treePrefab.GetComponentInChildren<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    // If it uses Unity's built-in Quad mesh
                    if (mf.sharedMesh.name == "Quad" || mf.sharedMesh.bounds.center.y == 0)
                    {
                        Mesh newMesh = Instantiate(mf.sharedMesh);
                        Vector3[] verts = newMesh.vertices;

                        // Shift mesh vertices up by 0.5 relative to unit space
                        for (int v = 0; v < verts.Length; v++)
                        {
                            verts[v].y += 0.5f;
                        }

                        newMesh.vertices = verts;
                        newMesh.RecalculateBounds();
                        mf.sharedMesh = newMesh;
                    }
                }
            }
        }

        // Force Terrain to refresh its tree instance render cache
        data.treePrototypes = prototypes;
        terrain.Flush();

        Debug.Log("Terrain trees successfully elevated!");
    }

    private void Start()
    {
        FixTrees();
    }
}