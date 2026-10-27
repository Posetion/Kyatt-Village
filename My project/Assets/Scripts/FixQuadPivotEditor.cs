using UnityEngine;
using UnityEditor;

public class FixQuadPivotEditor : EditorWindow
{
    [MenuItem("Tools/Fix Tree Mesh Pivot")]
    public static void LiftTreeMesh()
    {
        // Select your Tree prefab in the Project window first
        GameObject selectedPrefab = Selection.activeGameObject;

        if (selectedPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select the 'Tree' prefab in your Project window first!", "OK");
            return;
        }

        MeshFilter mf = selectedPrefab.GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            EditorUtility.DisplayDialog("Error", "No MeshFilter found on the selected prefab.", "OK");
            return;
        }

        // Create a copy of the mesh so we don't destroy default Unity assets
        Mesh newMesh = Object.Instantiate(mf.sharedMesh);
        Vector3[] vertices = newMesh.vertices;

        // Shift all vertices up by 0.5 units (moves origin from center to bottom base)
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y += 0.5f;
        }

        newMesh.vertices = vertices;
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        // Save new mesh asset into project
        string assetPath = "Assets/LiftedTreeMesh.asset";
        AssetDatabase.CreateAsset(newMesh, assetPath);
        AssetDatabase.SaveAssets();

        // Assign new mesh to prefab
        mf.sharedMesh = newMesh;
        EditorUtility.SetDirty(selectedPrefab);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Success!", "Mesh pivot moved to bottom! Now click 'Refresh' in the Terrain Tree Inspector.", "OK");
    }
}