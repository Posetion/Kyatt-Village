using UnityEngine;
using UnityEditor;

public static class InspectTerrainGrass
{
    public static string Inspect()
    {
        string[] guids =
        {
            "6a31112daf6579a458dae356ce5bbefd",
            "1bc793e6b895f7646ab7831f65de4159"
        };

        System.Text.StringBuilder report = new System.Text.StringBuilder();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TerrainData data = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
            report.Append(guid).Append("|").Append(path).Append("|");
            if (data == null)
            {
                report.Append("missing;");
                continue;
            }

            for (int i = 0; i < data.detailPrototypes.Length; i++)
            {
                DetailPrototype detail = data.detailPrototypes[i];
                report.Append(i).Append(":")
                    .Append(detail.prototype != null ? detail.prototype.name : "null").Append(":")
                    .Append(detail.usePrototypeMesh).Append(":")
                    .Append(detail.renderMode).Append(":")
                    .Append(detail.minWidth).Append("x").Append(detail.minHeight).Append(";");

                int[,] map = data.GetDetailLayer(
                    0,
                    0,
                    data.detailWidth,
                    data.detailHeight,
                    i);
                int nonzero = 0;
                int max = 0;
                foreach (int value in map)
                {
                    if (value > 0)
                        nonzero++;
                    if (value > max)
                        max = value;
                }
                report.Append("paint=").Append(nonzero).Append("/").Append(max).Append(";");
            }
        }

        return report.ToString();
    }
}
