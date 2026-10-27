using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InteractableChest : MonoBehaviour
{
    [Header("Interaction")]
    public float interactRange = 3.5f;
    public bool loadNextRoomOnOpen = true;
    public float nextRoomDelay = 1.6f;

    Animator animator;
    Transform player;
    Camera playerCamera;
    bool isOpen;
    bool playerInRange;

    void Start()
    {
        animator = GetComponent<Animator>();
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
            playerObject = GameObject.Find("PlayerArmature");

        if (playerObject != null)
            player = playerObject.transform;

        playerCamera = Camera.main;
    }

    void Update()
    {
        if (isOpen)
            return;

        if (player == null)
        {
            FindPlayer();
            if (player == null)
                return;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        playerInRange = toPlayer.magnitude <= interactRange;

        if (!playerInRange)
            return;

        if (WasInteractPressed() || ClickedThisChest())
            Open();
    }

    void LateUpdate()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            return;

        Vector3 look = transform.position - playerCamera.transform.position;
        look.y = 0f;
        if (look.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(look);
    }

    bool WasInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            return true;
#endif
        return Input.GetKeyDown(KeyCode.E);
    }

    bool ClickedThisChest()
    {
        bool clicked = false;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            clicked = true;
#endif
        if (!clicked)
            clicked = Input.GetMouseButtonDown(0);

        if (!clicked)
            return false;

        if (playerCamera == null)
            playerCamera = Camera.main;
        if (playerCamera == null)
            return false;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
#endif
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            return hit.transform == transform || hit.transform.IsChildOf(transform);

        return false;
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (animator != null)
            animator.Play("Open", 0, 0f);

        if (loadNextRoomOnOpen)
            StartCoroutine(LoadNextRoomAfterDelay());
    }

    IEnumerator LoadNextRoomAfterDelay()
    {
        yield return new WaitForSeconds(nextRoomDelay);

        if (SceneController.Instance != null)
            SceneController.Instance.LoadNextRoom();
    }
}
