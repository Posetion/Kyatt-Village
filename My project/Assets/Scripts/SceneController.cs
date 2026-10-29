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
    private bool isLoadingNextRoom;

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
        isLoadingNextRoom = false;
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
        if (isLoadingNextRoom) return;

        int nextRoomCount = roomCount + 1;
        bool goingToShop = roomsBeforeShop > 0 && nextRoomCount % roomsBeforeShop == 0;
        string destination = goingToShop ? shopScene : ChooseRandomRoom();
        if (string.IsNullOrEmpty(destination)) return;

        isLoadingNextRoom = true;

        if (SceneManager.GetActiveScene().name != shopScene &&
            GameManager.Instance != null &&
            GameManager.Instance.AreAllEnemiesDefeated)
        {
            GameManager.Instance.AwardPermanentUpgradePointForCompletedMap();
        }

        roomCount = nextRoomCount;
        Debug.Log("Room completed: " + roomCount);

        // Shop every X rooms
        if (goingToShop)
        {
            Debug.Log("Going to Shop!");
        }

        SceneManager.LoadScene(destination);
    }

    // ==========================================
    // RANDOM ROOM (NO REPEATS OF CURRENT MAP)
    // ==========================================

    private void LoadRandomRoom()
    {
        string chosenRoom = ChooseRandomRoom();
        if (!string.IsNullOrEmpty(chosenRoom))
        {
            SceneManager.LoadScene(chosenRoom);
        }
    }

    private string ChooseRandomRoom()
    {
        if (roomScenes == null || roomScenes.Length == 0)
        {
            Debug.LogError("No room scenes assigned in SceneController!");
            return null;
        }

        // If there's only 1 room available in the list, it has to load that one
        if (roomScenes.Length == 1)
        {
            return roomScenes[0];
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
        return chosenRoom;
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
        isLoadingNextRoom = false;
    }

    // ==========================================
    // START NEW RUN
    // ==========================================

   public void StartNewRun()
    {
        roomCount = 0;
        isLoadingNextRoom = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetPlayerStatsForNewRun();
        }
        else
        {
            Debug.LogError("Cannot start a new run because GameManager.Instance is missing.");
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