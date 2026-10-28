using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnStyle
    {
        SpawnAllCategories,
        PickRandomCategory,
        RareSingleEnemyOverride
    }

    [Header("Spawn Configuration")]
    public SpawnStyle spawnStyle = SpawnStyle.PickRandomCategory;

    [Header("Scaling Per Room")]
    [Tooltip("How many extra enemies are added to min/max counts for every room completed.")]
    [SerializeField] private int enemiesAddedPerRoom = 2;

    [Header("Rare Single Enemy Settings")]
    [Range(0f, 100f)]
    public float singleSpawnChance = 1f;

    [Header("Enemy List (Regular)")]
    public GameObject[] enemyPrefabs;
    public int minEnemies = 9;
    public int maxEnemies = 15;

    [Header("Fireball Settings")]
    public GameObject[] fireballPrefabs;
    public int minFireballs = 1;
    public int maxFireballs = 2;

    [Header("Tank Enemy Settings")]
    public GameObject[] tankPrefabs; 
    [Range(0f, 100f)]
    public float tankSpawnChance = 100f; 
    public int minTanks = 1; 
    public int maxTanks = 3;

    [Header("Elite / Boss Settings")]
    public GameObject[] elitePrefabs; 
    [Range(0f, 100f)]
    public float eliteSpawnChance = 15f;
    public int minElites = 1;
    public int maxElites = 1;

    [Header("General Settings")]
    public float spawnRadius = 3f;

    private int aliveEnemyCount = 0;
    private bool warnedNavMeshUnavailable;

    void Start()
    {
        SpawnWave();
    }

    public void SpawnWave()
    {
        aliveEnemyCount = 0;

        // Determine scaling based on current room count from SceneController
        int roomBonus = 0;
        if (SceneController.Instance != null)
        {
            roomBonus = SceneController.Instance.GetRoomCount() * enemiesAddedPerRoom;
        }

        bool hasRegulars = enemyPrefabs != null && enemyPrefabs.Length > 0 && enemyPrefabs[0] != null;
        bool hasFireballs = fireballPrefabs != null && fireballPrefabs.Length > 0 && fireballPrefabs[0] != null;
        bool hasTanks = tankPrefabs != null && tankPrefabs.Length > 0 && tankPrefabs[0] != null;
        bool hasElites = elitePrefabs != null && elitePrefabs.Length > 0 && elitePrefabs[0] != null;

        switch (spawnStyle)
        {
            case SpawnStyle.SpawnAllCategories:
                if (hasRegulars) SpawnRegularEnemies(roomBonus);
                if (hasFireballs) SpawnFireballs(roomBonus);
                if (hasTanks) TrySpawnTanks(roomBonus);
                if (hasElites) TrySpawnElites(roomBonus);
                break;

            case SpawnStyle.PickRandomCategory:
                var validCategories = new System.Collections.Generic.List<int>();
                if (hasRegulars) validCategories.Add(0);
                if (hasFireballs) validCategories.Add(1);
                if (hasTanks) validCategories.Add(2);
                if (hasElites) validCategories.Add(3);

                if (validCategories.Count == 0) break;

                int chosenCategory = validCategories[Random.Range(0, validCategories.Count)];
                if (chosenCategory == 0) SpawnRegularEnemies(roomBonus);
                else if (chosenCategory == 1) SpawnFireballs(roomBonus);
                else if (chosenCategory == 2) TrySpawnTanks(roomBonus);
                else if (chosenCategory == 3) TrySpawnElites(roomBonus);
                break;

            case SpawnStyle.RareSingleEnemyOverride:
                float roll = Random.Range(0f, 100f);
                if (roll <= singleSpawnChance)
                {
                    SpawnSingleRandomEnemyType(roomBonus);
                }
                else
                {
                    if (hasRegulars) SpawnRegularEnemies(roomBonus);
                    if (hasFireballs) SpawnFireballs(roomBonus);
                    if (hasTanks) TrySpawnTanks(roomBonus);
                    if (hasElites) TrySpawnElites(roomBonus);
                }
                break;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterWaveEnemies(aliveEnemyCount);
        }
    }

    private void SpawnRegularEnemies(int bonus)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        int count = Random.Range(minEnemies + bonus, maxEnemies + bonus + 1);
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            if (prefab != null) SpawnObject(prefab);
        }
    }

    private void SpawnFireballs(int bonus)
    {
        if (fireballPrefabs == null || fireballPrefabs.Length == 0) return;
        // Scale fireballs slightly slower or by half the bonus if desired, or use full bonus
        int scaledMin = minFireballs + (bonus / 2);
        int scaledMax = maxFireballs + (bonus / 2);
        int count = Random.Range(scaledMin, scaledMax + 1);
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = fireballPrefabs[Random.Range(0, fireballPrefabs.Length)];
            if (prefab != null) SpawnObject(prefab);
        }
    }

    private void TrySpawnTanks(int bonus)
    {
        if (tankPrefabs == null || tankPrefabs.Length == 0) return;

        if (Random.Range(0f, 100f) <= tankSpawnChance)
        {
            int count = Random.Range(minTanks, maxTanks + 1 + (bonus / 3));
            for (int i = 0; i < count; i++)
            {
                GameObject prefab = tankPrefabs[Random.Range(0, tankPrefabs.Length)];
                if (prefab != null) SpawnObject(prefab);
            }
        }
    }

    private void TrySpawnElites(int bonus)
    {
        if (elitePrefabs == null || elitePrefabs.Length == 0) return;

        // Optionally increase elite spawn chance slightly as rooms increase, or keep it flat
        float adjustedChance = eliteSpawnChance + (SceneController.Instance != null ? SceneController.Instance.GetRoomCount() * 1f : 0f);

        if (Random.Range(0f, 100f) <= Mathf.Min(adjustedChance, 50f)) // Cap at 50% max chance
        {
            int count = Random.Range(minElites, maxElites + 1);
            for (int i = 0; i < count; i++)
            {
                GameObject prefab = elitePrefabs[Random.Range(0, elitePrefabs.Length)];
                if (prefab != null) SpawnObject(prefab);
            }
        }
    }

    private void SpawnSingleRandomEnemyType(int bonus)
    {
        bool hasRegulars = enemyPrefabs != null && enemyPrefabs.Length > 0 && enemyPrefabs[0] != null;
        bool hasFireballs = fireballPrefabs != null && fireballPrefabs.Length > 0 && fireballPrefabs[0] != null;
        bool hasTanks = tankPrefabs != null && tankPrefabs.Length > 0 && tankPrefabs[0] != null;
        bool hasElites = elitePrefabs != null && elitePrefabs.Length > 0 && elitePrefabs[0] != null;

        var availableTypes = new System.Collections.Generic.List<int>();
        if (hasRegulars) availableTypes.Add(0); 
        if (hasFireballs) availableTypes.Add(1); 
        if (hasTanks) availableTypes.Add(2); 
        if (hasElites) availableTypes.Add(3); 

        if (availableTypes.Count == 0) return;

        int selectedType = availableTypes[Random.Range(0, availableTypes.Count)];

        switch (selectedType)
        {
            case 0:
                {
                    GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                    int count = Random.Range(minEnemies + bonus, maxEnemies + bonus + 1);
                    for (int i = 0; i < count; i++) SpawnObject(prefab);
                    break;
                }
            case 1:
                {
                    GameObject prefab = fireballPrefabs[Random.Range(0, fireballPrefabs.Length)];
                    int count = Random.Range(minEnemies + bonus, maxEnemies + bonus + 1);
                    for (int i = 0; i < count; i++) SpawnObject(prefab);
                    break;
                }
            case 2:
                {
                    GameObject prefab = tankPrefabs[Random.Range(0, tankPrefabs.Length)];
                    int count = Random.Range(minTanks, maxTanks + 1); 
                    for (int i = 0; i < count; i++) SpawnObject(prefab);
                    break;
                }
            case 3:
                {
                    GameObject prefab = elitePrefabs[Random.Range(0, elitePrefabs.Length)];
                    int count = Random.Range(minElites, maxElites + 1);
                    for (int i = 0; i < count; i++) SpawnObject(prefab);
                    break;
                }
        }
    }

    private void SpawnObject(GameObject prefabToSpawn)
    {
        NavMeshAgent prefabAgent = prefabToSpawn.GetComponent<NavMeshAgent>();
        if (prefabAgent != null)
        {
            NavMeshQueryFilter filter = new NavMeshQueryFilter
            {
                agentTypeID = prefabAgent.agentTypeID,
                areaMask = prefabAgent.areaMask
            };

            float sampleDistance = Mathf.Max(spawnRadius + 1f, 2f);
            bool foundSpawnPosition = false;
            Vector3 spawnPosition = transform.position;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                Vector2 randomDirection = Random.insideUnitCircle * spawnRadius;
                Vector3 candidatePosition = transform.position + new Vector3(randomDirection.x, 0f, randomDirection.y);
                if (!NavMesh.SamplePosition(candidatePosition, out NavMeshHit hit, sampleDistance, filter))
                {
                    continue;
                }

                spawnPosition = hit.position;
                foundSpawnPosition = true;
                break;
            }

            if (!foundSpawnPosition)
            {
                if (!warnedNavMeshUnavailable)
                {
                    Debug.LogWarning(gameObject.name + " could not find a reachable NavMesh spawn position near the player. Check the room's baked NavMesh data.", this);
                    warnedNavMeshUnavailable = true;
                }

                return;
            }

            warnedNavMeshUnavailable = false;
            Instantiate(prefabToSpawn, spawnPosition, transform.rotation);
            aliveEnemyCount++;
            return;
        }

        Vector2 fireballDirection = Random.insideUnitCircle * spawnRadius;
        Vector3 fireballSpawnPosition = transform.position + new Vector3(fireballDirection.x, 0f, fireballDirection.y);
        Instantiate(prefabToSpawn, fireballSpawnPosition, transform.rotation);

        aliveEnemyCount++;
    }
}