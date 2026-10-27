
using UnityEngine;

public class EliteEnemy : MonoBehaviour
{
    [Header("Elite Appearance")]
    public Transform visuals;
    public Renderer[] enemyRenderers;
    public Color eliteColor = Color.red;
    public float eliteScale = 1.4f;

    [Header("Elite Stats")]
    public float normalHealth = 100f;
    public float normalDamage = 10f;
    public float normalSpeed = 3f;

    public float healthMultiplier = 2f;
    public float damageMultiplier = 1.5f;
    public float speedMultiplier = 1.2f;

    public float MaxHealth { get; private set; }
    public float Damage { get; private set; }
    public float MoveSpeed { get; private set; }

    private bool isElite;

    public void MakeElite(float difficulty)
    {
        if (isElite) return;
        isElite = true;

        float healthScale = 1f + difficulty * 0.15f;
        float damageScale = 1f + difficulty * 0.08f;

        MaxHealth = normalHealth * healthMultiplier * healthScale;
        Damage = normalDamage * damageMultiplier * damageScale;
        MoveSpeed = normalSpeed * speedMultiplier;

        if (visuals != null)
            visuals.localScale *= eliteScale;

        foreach (Renderer rend in enemyRenderers)
        {
            if (rend == null) continue;

            foreach (Material mat in rend.materials)
            {
                mat.color = eliteColor;
            }
        }
    }
}