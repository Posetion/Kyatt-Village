using StarterAssets;
using UnityEngine;

public class VirtualInput : MonoBehaviour
{
    [Header("Output")]
    public StarterAssetsInputs StarterAssetsInputs;

    private void OnEnable()
    {
        // Try to find the player automatically whenever the UI opens or a new scene loads
        FindAndBindPlayer();
    }

    private void Start()
    {
        FindAndBindPlayer();
    }

    public void FindAndBindPlayer()
    {
        // If reference is already missing or null, search for it
        if (StarterAssetsInputs == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player == null)
            {
                // Fallback: search by name if the tag isn't set on the player
                player = GameObject.Find("PlayerArmature"); 
            }

            if (player != null)
            {
                StarterAssetsInputs = player.GetComponent<StarterAssetsInputs>();
            }
        }
    }

    public void VirtualMoveInput(Vector2 virtualMoveDirection)
    {
        if (StarterAssetsInputs != null) StarterAssetsInputs.MoveInput(virtualMoveDirection);
    }

    public void VirtualLookInput(Vector2 virtualLookDirection)
    {
        if (StarterAssetsInputs != null) StarterAssetsInputs.LookInput(virtualLookDirection);
    }

    public void VirtualJumpInput(bool virtualJumpState)
    {
        if (StarterAssetsInputs != null) StarterAssetsInputs.JumpInput(virtualJumpState);
    }

    public void VirtualSprintInput(bool virtualSprintState)
    {
        if (StarterAssetsInputs != null) StarterAssetsInputs.SprintInput(virtualSprintState);
    }
}