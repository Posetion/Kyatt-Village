using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VirtualInput : MonoBehaviour
{
    [Header("Output")]
    public StarterAssetsInputs StarterAssetsInputs;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindAndBindPlayer();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Force re-binding whenever a new scene/map is fully loaded
        FindAndBindPlayer();
    }

    private void Start()
    {
        FindAndBindPlayer();
    }

    public void FindAndBindPlayer()
    {
        // Always search if the reference is null OR if the reference points to a destroyed object from a previous scene
        if (StarterAssetsInputs == null || StarterAssetsInputs.gameObject == null)
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
                Debug.Log($"[VirtualInput] Successfully bound to player: {player.name}");
            }
            else
            {
                Debug.LogWarning("[VirtualInput] Could not find any GameObject with tag 'Player' or name 'PlayerArmature' in the new scene!");
            }
        }
    }

    public void VirtualMoveInput(Vector2 virtualMoveDirection)
    {
        // Re-check binding on the fly just in case it dropped
        if (StarterAssetsInputs == null) FindAndBindPlayer();

        if (StarterAssetsInputs != null) StarterAssetsInputs.MoveInput(virtualMoveDirection);
    }

    public void VirtualLookInput(Vector2 virtualLookDirection)
    {
        if (StarterAssetsInputs == null) FindAndBindPlayer();

        if (StarterAssetsInputs != null) StarterAssetsInputs.LookInput(virtualLookDirection);
    }

    public void VirtualJumpInput(bool virtualJumpState)
    {
        if (StarterAssetsInputs == null) FindAndBindPlayer();

        if (StarterAssetsInputs != null) StarterAssetsInputs.JumpInput(virtualJumpState);
    }

    public void VirtualSprintInput(bool virtualSprintState)
    {
        if (StarterAssetsInputs == null) FindAndBindPlayer();

        if (StarterAssetsInputs != null) StarterAssetsInputs.SprintInput(virtualSprintState);
    }
}