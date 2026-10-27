
using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Camera Position")]
    public Vector3 cameraOffset = new Vector3(0f, 5f, -8f);

    [Header("Camera Rotation")]
    public Vector3 cameraRotation = new Vector3(25f, 0f, 0f);

    [Header("Follow Settings")]
    public float followSpeed = 5f;
    public bool followX = true;
    public bool followY = true;
    public bool followZ = true;

    [Header("Look At Player")]
    public bool lookAtPlayer = false;
    public float lookAtHeight = 1f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject == null)
                playerObject = GameObject.Find("PlayerArmature");

            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        // Target camera position
        Vector3 targetPosition = player.position + cameraOffset;

        // Keep current camera position if a direction is disabled
        if (!followX)
            targetPosition.x = transform.position.x;

        if (!followY)
            targetPosition.y = transform.position.y;

        if (!followZ)
            targetPosition.z = transform.position.z;

        // Smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // Camera rotation
        if (lookAtPlayer)
        {
            Vector3 lookDirection =
                (player.position + Vector3.up * lookAtHeight) - transform.position;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(lookDirection);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    followSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            transform.rotation = Quaternion.Euler(cameraRotation);
        }
    }
}


