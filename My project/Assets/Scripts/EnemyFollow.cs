using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 3f;

    void Update()
    {
        if (player == null)
            return;

        // Find direction toward the player
        Vector3 direction = (player.position - transform.position).normalized;

        // Move toward player
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}