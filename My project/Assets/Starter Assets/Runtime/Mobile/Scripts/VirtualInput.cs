using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VirtualInput : MonoBehaviour
{
    public static VirtualInput Instance;

    [Header("Output")]
    public StarterAssetsInputs StarterAssetsInputs;

    private void Awake()
    {
        // Setup a persistent singleton so mobile buttons NEVER lose their target across scenes
        if (Instance == null)
        {
            Instance = this;
            StarterAssetsInputs = null;
            DontDestroyOnLoad(gameObject); // <--- THIS IS THE MAGIC FIX FOR MOBILE BUTTONS!
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Clear reference on scene load and search for the new player in the new map
        StarterAssetsInputs = null;
        FindAndBindPlayer();
    }

    // Called automatically by the Player when they spawn in a new scene!
    public static void RegisterPlayer(StarterAssetsInputs newPlayerInputs)
    {
        if (Instance != null && newPlayerInputs != null)
        {
            Instance.StarterAssetsInputs = newPlayerInputs;
            Debug.Log($"[VirtualInput] Player successfully auto-registered: {newPlayerInputs.gameObject.name}");
        }
    }

    public void FindAndBindPlayer()
    {
        if (StarterAssetsInputs == null || !StarterAssetsInputs.gameObject.scene.IsValid())
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("PlayerArmature");

            if (player != null)
            {
                StarterAssetsInputs = player.GetComponent<StarterAssetsInputs>();
                if (StarterAssetsInputs == null)
                {
                    StarterAssetsInputs = player.GetComponentInChildren<StarterAssetsInputs>();
                }

                Debug.Log($"[VirtualInput] Found and bound player in scene: {player.name}");
            }
        }
    }

    public void VirtualMoveInput(Vector2 virtualMoveDirection)
    {
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