using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("Normal Room Scenes")]
    [SerializeField] private string[] roomScenes;

    [Header("Special Scenes")]
    [SerializeField] private string shopScene = "Shop";

    [Header("Room Settings")]
    [SerializeField] private int roomsBeforeShop = 5;

    private int roomCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // NORMAL SCENE LOADING
    // ==========================================

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ==========================================
    // LOAD NEXT ROOM
    // ==========================================

    public void LoadNextRoom()
    {
        roomCount++;
        Debug.Log("Room completed: " + roomCount);


        // Shop every X rooms
        if (roomCount % roomsBeforeShop == 0)
        {
            Debug.Log("Going to Shop!");
            SceneManager.LoadScene(shopScene);
            return;
        }

        // Random normal room (avoiding repeat of current room)
        LoadRandomRoom();
    }

    // ==========================================
    // RANDOM ROOM (NO REPEATS OF CURRENT MAP)
    // ==========================================

    private void LoadRandomRoom()
    {
        if (roomScenes == null || roomScenes.Length == 0)
        {
            Debug.LogError("No room scenes assigned in SceneController!");
            return;
        }

        // If there's only 1 room available in the list, it has to load that one
        if (roomScenes.Length == 1)
        {
            SceneManager.LoadScene(roomScenes[0]);
            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        string chosenRoom;
        int safetyLoop = 0; // Prevents infinite loops

        do
        {
            int randomIndex = Random.Range(0, roomScenes.Length);
            chosenRoom = roomScenes[randomIndex];
            safetyLoop++;

            // Keep picking randomly until we find a room that does NOT match the current map
        } while (chosenRoom == currentSceneName && safetyLoop < 100);

        Debug.Log("Loading random room: " + chosenRoom);
        SceneManager.LoadScene(chosenRoom);
    }

    // ==========================================
    // GET ROOM COUNT
    // ==========================================

    public int GetRoomCount()
    {
        return roomCount;
    }

    // ==========================================
    // RESET RUN
    // ==========================================

    public void ResetRooms()
    {
        roomCount = 0;
    }

    // ==========================================
    // START NEW RUN
    // ==========================================

   public void StartNewRun()
    {
        roomCount = 0;

        // Reset player stats, health, and timer cleanly at the start of a brand new run
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerMaxHealth = 1000f;
            GameManager.Instance.playerCurrentHealth = 1000f;
            GameManager.Instance.totalCoins = 0;
            GameManager.Instance.collectedPowerUpIcons.Clear();
            GameManager.Instance.levelElapsedTime = 0f; // <-- Added timer reset
        }

        LoadRandomRoom();
    }
    // ==========================================
    // OTHER SCENE FUNCTIONS
    // ==========================================

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
    }
}