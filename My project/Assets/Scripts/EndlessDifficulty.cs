
using UnityEngine;

public class EndlessDifficulty : MonoBehaviour
{
    public int stage = 1;

    public float Difficulty =>
        Mathf.Max(0, stage - 1);

    public float EliteChance =>
        Mathf.Clamp(0.05f + Difficulty * 0.02f, 0.05f, 0.50f);

    public void AdvanceStage()
    {
        stage++;
        Debug.Log("Endless Stage: " + stage);
    }
}