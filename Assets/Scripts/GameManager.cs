using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using AIAgent = Task_4.AI.AIAgent;
using Arrive = Task_4.AI.Arrive;
using Avoidance = Task_4.AI.Avoidance;
using FaceDirection = Task_4.AI.FaceDirection;
using Flee = Task_4.AI.Flee;
using FOVTrigger = Task_4.AI.FOVTrigger;
using MeshGenerator = Task_4.AI.MeshGenerator;
using Pursue = Task_4.AI.Pursue;
using Sneak = Task_4.AI.Sneak;
using Wander = Task_4.AI.Wander;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    _instance = obj.AddComponent<GameManager>();
                    DontDestroyOnLoad(obj);
                }
            }

            return _instance;
        }
    }
    
    [Header("Game Settings")]
    [SerializeField] private float totalTime;
    [SerializeField] private float maxNumberOfFishingShips;
    [SerializeField] private List<Material> materials;
    [SerializeField] private GameObject tradeShipPrefab;
    [SerializeField] private GameObject fishingShipPrefab;
    [SerializeField] private Path pathTemplate;
    private int _totalNumberOfPirates;
    
    [Header("Trade Ship Settings")]
    [SerializeField] private float tradeShipMaxSpeed; // 3
    [SerializeField] private float viewDistance; // 15
    [SerializeField] private float tradeShipAvoidanceRadius; // 2.5
    [SerializeField] private float tradeShipAvoidanceFactor; // 5
    [SerializeField] private float arriveSlowRadius; // 10
    [SerializeField] private float arriveStopRadius; // 5
    [SerializeField] private float seekWeight; // 0.5
    [SerializeField] private float velocityMatchWeight; // 1
    [SerializeField] private float sneakBufferDistance; // 10
    [SerializeField] private float sneakSlowRadius; // 12 
    [SerializeField] private float sneakCrowdingSeparation; // 1
    [SerializeField] private float spawnRadius;
    
    [Header("Pirate Ship Settings")]
    [SerializeField] private float pirateShipMaxSpeed; // 1.5
    [SerializeField] private float fovDistance; // 75
    [SerializeField] private float fovAngle; // 120
    [SerializeField] private int segments; // 20
    [SerializeField] private float pirateShipAvoidanceRadius; // 4
    [SerializeField] private float pirateShipAvoidanceFactor; // 2
    [SerializeField] private float wanderDegrees; // 90
    [SerializeField] private float wanderInterval; // 3
    [SerializeField] private float orbitRadius; // 15
    [SerializeField] private float correctionFactor; // 0.5
    
    [Header("Fishing Ship Settings")]
    [SerializeField] private float fishingShipMaxSpeed;
    [SerializeField] private float fishingRadius;
    [SerializeField] private float fishingSpawnRadius;
    
    [Header("Barrel Settings")]
    [SerializeField] private float barrelRadius;
    [SerializeField] private float barrelMaxSpeed;
    [SerializeField] private float barrelWanderInterval;
    [SerializeField] private float barrelWanderDegree;
    [SerializeField] private float barrelCorrectionFactor;
    
    [Header("Objects in Scene")]
    [SerializeField] private List<GameObject> pirateShips = new List<GameObject>();
    [SerializeField] private List<GameObject> tradeShips = new List<GameObject>();
    [SerializeField] private List<GameObject> harbors = new List<GameObject>();
    [SerializeField] private List<GameObject> barrels = new List<GameObject>();
    [SerializeField] private List<GameObject> fishingShips = new List<GameObject>();
    [SerializeField] private List<GameObject> environment = new List<GameObject>();
    [SerializeField] private GameObject island;
    
    
    [Header("General Ship Settings")]
    [SerializeField] private bool lockY;
    [SerializeField] private bool debug;
    private Transform _fovTransform;
    private GameObject _fovObject;
    

    private float _gameTimer;
    private bool _gameOver;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            FetchShips();
        }
        else if (_instance != this)
        {
            // Destroy only if it's a different GameManager in the same scene
            if (_instance.gameObject.scene == gameObject.scene)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        // Only persist if we want this GameManager to be the same across scenes
        // if (ShouldPersistAcrossScenes(SceneManager.GetActiveScene().name))
        // {
        //     DontDestroyOnLoad(gameObject);
        // }
    }
    
    private bool ShouldPersistAcrossScenes(string sceneName)
    {
        // Define scenes where GameManager should persist
        return sceneName == "MainMenu" || sceneName == "Task 4" || sceneName == "Task 5" || sceneName == "Task 6" || sceneName == "Task 7" || sceneName == "Task 8" || sceneName == "Task 9" || sceneName == "Task 10";
    }

    private void Update()
    {
        DebugUtils.DrawCircle(island.transform.position, Vector3.up, Color.blue, barrelRadius);
        if (!_gameOver)
        {
            _gameTimer += Time.deltaTime;
            CheckGameOver();
        }
    }

    public Path GetPath()
    {
        return pathTemplate;
    }

    public List<GameObject> GetPirates()
    {
        return new List<GameObject>(pirateShips);
    }
        
    public void AddPirate(GameObject pirate)
    {
        if (pirate != null && !pirateShips.Contains(pirate))
        {
            pirateShips.Add(pirate);
        }
    }
        
    public void RemovePirate(GameObject pirate)
    {
        if (pirate != null && pirateShips.Contains(pirate))
        {
            pirateShips.Remove(pirate);
        }
    }
    
    public void RemoveFishingShip(GameObject ship)
    {
        if (ship != null && fishingShips.Contains(ship))
        {
            fishingShips.Remove(ship);
        }
    }

    public int GetAlivePirates()
    {
        return pirateShips.Count;
    }

    public int GetTotalPirates()
    {
        return _totalNumberOfPirates;
    }

    public List<GameObject> GetTradeShips()
    {
        return new List<GameObject>(tradeShips);
    }

    public List<GameObject> GetBarrels()
    {
        return new List<GameObject>(barrels);
    }

    public List<GameObject> GetFishingShips()
    {
        return new List<GameObject>(fishingShips);
    }
    public float GetTotalTime()
    {
        return totalTime;
    }

    public float GetRemainingTime()
    {
        return totalTime - _gameTimer;
    }
        
    public void RemoveTradeShip(GameObject tradeShip)
    {
        if (tradeShip != null && tradeShips.Contains(tradeShip))
        {
            tradeShips.Remove(tradeShip);
        }
    }
    private void CheckGameOver()
    {
        if (tradeShips.Count == 0)
        {
            GameOver("Pirates Win! All trade ships are destroyed.");
            Time.timeScale = 0;
        }
        else if (_gameTimer >= totalTime) 
        {
            GameOver("Time's up! Game over.");
            Time.timeScale = 0;
        }
    }
        
    private void GameOver(string message)
    {
        _gameOver = true;
        Debug.Log(message);
    }

    private void FetchShips()
    {
        pirateShips.Clear();
        tradeShips.Clear();
        harbors.Clear();
        barrels.Clear();
        fishingShips.Clear();
        environment.Clear();
        
        pirateShips.AddRange(GameObject.FindGameObjectsWithTag("PirateShip"));
        harbors.AddRange(GameObject.FindGameObjectsWithTag("Port"));
        barrels.AddRange(GameObject.FindGameObjectsWithTag("Barrel"));
        environment.AddRange(GameObject.FindGameObjectsWithTag("Island"));
        if (materials.Count <= 0)
        {
            tradeShips.AddRange(GameObject.FindGameObjectsWithTag("TradeShip"));
        }
        else
        {
            foreach (GameObject harbor in harbors)
            {
                Renderer renderer = harbor.GetComponent<Renderer>();

                if (renderer != null)
                {
                    if (materials.Contains(renderer.sharedMaterial))
                    {
                        // Debug.Log($"Harbor '{harbor.name}' contains material '{renderer.material.name}'");
                        GameObject tradeShip = Spawn(harbor, renderer.sharedMaterial);
                        if (tradeShip == null || harbor == null)
                        {
                            Debug.LogWarning("Could not find trade ship.");
                        }
                        else
                        {
                            CreateTeam(harbor, tradeShip);
                        }
                    }
                }
            }
            tradeShips.AddRange(GameObject.FindGameObjectsWithTag("TradeShip"));
            
            
            fishingShips.AddRange(GameObject.FindGameObjectsWithTag("FishingShip"));
        }
        island = GameObject.FindGameObjectWithTag("Island");
        
        _totalNumberOfPirates = pirateShips.Count;
        
        string sceneName = SceneManager.GetActiveScene().name;

        ApplyScripts(sceneName);
    }
    private void CreateTeam(GameObject harbor, GameObject tradeShip)
    {
        Team team = ScriptableObject.CreateInstance<Team>();
        team.Harbor = harbor;
        team.TradeShip = tradeShip;
        ScoreManager.Instance.AddTeam(team);
    }

    public GameObject Spawn(GameObject harbor, Material material)
    {
        Vector3 spawnPosition = GetSpawnPoint(harbor.transform.position, spawnRadius);
        GameObject tradeShip = Instantiate(tradeShipPrefab, spawnPosition, Quaternion.identity);
        Transform sails = tradeShip.transform.Find("Sails");
        if (sails != null)
        {
            ChangeColour(sails, material);
        }
        else
        {
            Debug.LogWarning("Could not find sail");
        }
        return tradeShip;
    }

    public void ChangeColour(Transform sail, Material material)
    {
        foreach (Transform child in sail)
        {
            child.GetComponent<Renderer>().material = material;
        }
    }
    public void Respawn(Team team)
    {
        Vector3 spawnPosition = GetSpawnPoint(team.Harbor.transform.position, spawnRadius);
        Instantiate(team.TradeShip, spawnPosition, Quaternion.identity);
    }

    private static Vector3 GetSpawnPoint(Vector3 harborPosition, float radius)
    {
        Vector3 spawnPoint;
        RaycastHit hit;
        LayerMask groundMask = LayerMask.GetMask("Ground");
        LayerMask waterMask = LayerMask.GetMask("Water");

        int maxAttempts = 10; // Prevent infinite loops
        int attempt = 0;

        do
        {
            // Generate a random spawn offset in the x-z plane
            float angle = Random.Range(0f, 360f);
            float radians = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(radians) * radius;
            float z = Mathf.Sin(radians) * radius;

            // Start the spawn point a bit above the expected ground level
            spawnPoint = new Vector3(harborPosition.x + x, harborPosition.y + 10f, harborPosition.z + z);

            // Perform a downward raycast to check the terrain
            if (Physics.Raycast(spawnPoint, Vector3.down, out hit, 50f))
            {
                spawnPoint.y = hit.point.y + 0.9f;

                // Check if the hit surface is water
                if (((1 << hit.collider.gameObject.layer) & waterMask) != 0)
                {
                    return spawnPoint; // Valid water spawn found
                }
            }

            attempt++;
        } while (attempt < maxAttempts);

        // If no valid water spawn is found, return harbor position slightly above water as fallback
        return new Vector3(harborPosition.x, harborPosition.y + 0.15f, harborPosition.z);
    }


    private void ApplyScripts(string sceneName)
    {
        foreach (GameObject fishingShip in fishingShips)
        {
            switch (sceneName)
            {
                case "Task 7":
                    if (!fishingShip.GetComponent<Task_7.AI.AIAgent>())
                    {
                        fishingShip.AddComponent<Task_7.AI.AIAgent>();
                        Task_7.AI.AIAgent aiAgent = fishingShip.GetComponent<Task_7.AI.AIAgent>();
                        aiAgent.maxSpeed = fishingShipMaxSpeed;
                        aiAgent.island = island;
                        aiAgent.ports = harbors;
                    }

                    if (!fishingShip.GetComponent<Task_7.AI.PathFind>())
                    {
                        fishingShip.AddComponent<Task_7.AI.PathFind>();
                        Task_7.AI.PathFind aiAgent = fishingShip.GetComponent<Task_7.AI.PathFind>();
                        aiAgent.fishingRadius = fishingRadius;
                    }
                    
                    if (!fishingShip.GetComponent<Task_7.AI.FaceDirection>())
                    {
                        fishingShip.AddComponent<Task_7.AI.FaceDirection>();
                    }
                    
                    if (!fishingShip.GetComponent<Task_7.AI.Avoidance>())
                    {
                        fishingShip.AddComponent<Task_7.AI.Avoidance>();
                        Task_7.AI.Avoidance aiAgent = fishingShip.GetComponent<Task_7.AI.Avoidance>();
                        aiAgent.avoidanceRadius = tradeShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = pirateShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }
                    
                    break;
                case "Task 8":
                    if (!fishingShip.GetComponent<Task_8.AI.AIAgent>())
                    {
                        fishingShip.AddComponent<Task_8.AI.AIAgent>();
                        Task_8.AI.AIAgent aiAgent = fishingShip.GetComponent<Task_8.AI.AIAgent>();
                        aiAgent.maxSpeed = fishingShipMaxSpeed;
                        aiAgent.islands = environment;
                        aiAgent.ports = harbors;
                    }

                    if (!fishingShip.GetComponent<Task_8.AI.PathFind>())
                    {
                        fishingShip.AddComponent<Task_8.AI.PathFind>();
                        Task_8.AI.PathFind aiAgent = fishingShip.GetComponent<Task_8.AI.PathFind>();
                        aiAgent.fishingRadius = fishingRadius;
                    }
                    
                    if (!fishingShip.GetComponent<Task_8.AI.FaceDirection>())
                    {
                        fishingShip.AddComponent<Task_8.AI.FaceDirection>();
                    }
                    
                    if (!fishingShip.GetComponent<Task_8.AI.Avoidance>())
                    {
                        fishingShip.AddComponent<Task_8.AI.Avoidance>();
                        Task_8.AI.Avoidance aiAgent = fishingShip.GetComponent<Task_8.AI.Avoidance>();
                        aiAgent.avoidanceRadius = tradeShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = pirateShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }
                    
                    break;
                case "Task 9":
                    if (!fishingShip.GetComponent<Task_9.AI.AIAgent>())
                    {
                        fishingShip.AddComponent<Task_9.AI.AIAgent>();
                        Task_9.AI.AIAgent aiAgent = fishingShip.GetComponent<Task_9.AI.AIAgent>();
                        aiAgent.maxSpeed = fishingShipMaxSpeed;
                        aiAgent.islands = environment;
                        aiAgent.ports = harbors;
                    }

                    if (!fishingShip.GetComponent<Task_9.AI.PathFind>())
                    {
                        fishingShip.AddComponent<Task_9.AI.PathFind>();
                        Task_9.AI.PathFind aiAgent = fishingShip.GetComponent<Task_9.AI.PathFind>();
                        aiAgent.fishingRadius = fishingRadius;
                    }
                    
                    if (!fishingShip.GetComponent<Task_9.AI.FaceDirection>())
                    {
                        fishingShip.AddComponent<Task_9.AI.FaceDirection>();
                    }
                    
                    if (!fishingShip.GetComponent<Task_9.AI.Avoidance>())
                    {
                        fishingShip.AddComponent<Task_9.AI.Avoidance>();
                        Task_9.AI.Avoidance aiAgent = fishingShip.GetComponent<Task_9.AI.Avoidance>();
                        aiAgent.avoidanceRadius = tradeShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = pirateShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }
                    
                    break;
            }
        }
        
        foreach (GameObject barrel in barrels)
        {
            switch (sceneName)
            {
                case "Task 6":
                    if (!barrel.GetComponent<Task_6.AI.AIAgent>())
                    {
                        barrel.AddComponent<Task_6.AI.AIAgent>();
                        Task_6.AI.AIAgent aiAgent = barrel.GetComponent<Task_6.AI.AIAgent>();
                        aiAgent.maxSpeed = barrelMaxSpeed;
                        aiAgent.island = island;
                    }

                    if (!barrel.GetComponent<Task_6.AI.Wander>())
                    {
                        barrel.AddComponent<Task_6.AI.Wander>();
                        Task_6.AI.Wander aiAgent = barrel.GetComponent<Task_6.AI.Wander>();
                        aiAgent.wanderDegrees = barrelWanderDegree;
                        aiAgent.wanderInterval = barrelWanderInterval;
                        aiAgent.orbitRadius = barrelRadius;
                        aiAgent.correctionFactor = barrelCorrectionFactor;
                    }
                    
                    break;
                case "Task 7":
                    if (!barrel.GetComponent<Task_7.AI.AIAgent>())
                    {
                        barrel.AddComponent<Task_7.AI.AIAgent>();
                        Task_7.AI.AIAgent aiAgent = barrel.GetComponent<Task_7.AI.AIAgent>();
                        aiAgent.maxSpeed = barrelMaxSpeed;
                        aiAgent.island = island;
                    }

                    if (!barrel.GetComponent<Task_7.AI.Wander>())
                    {
                        barrel.AddComponent<Task_7.AI.Wander>();
                        Task_7.AI.Wander aiAgent = barrel.GetComponent<Task_7.AI.Wander>();
                        aiAgent.wanderDegrees = barrelWanderDegree;
                        aiAgent.wanderInterval = barrelWanderInterval;
                        aiAgent.orbitRadius = barrelRadius;
                        aiAgent.correctionFactor = barrelCorrectionFactor;
                    }
                    
                    break;
                case "Task 8":
                    if (!barrel.GetComponent<Task_8.AI.AIAgent>())
                    {
                        barrel.AddComponent<Task_8.AI.AIAgent>();
                        Task_8.AI.AIAgent aiAgent = barrel.GetComponent<Task_8.AI.AIAgent>();
                        aiAgent.maxSpeed = barrelMaxSpeed;
                        aiAgent.islands = environment;
                    }

                    if (!barrel.GetComponent<Task_8.AI.Wander>())
                    {
                        barrel.AddComponent<Task_8.AI.Wander>();
                        Task_8.AI.Wander aiAgent = barrel.GetComponent<Task_8.AI.Wander>();
                        aiAgent.wanderDegrees = barrelWanderDegree;
                        aiAgent.wanderInterval = barrelWanderInterval;
                        aiAgent.orbitRadius = barrelRadius;
                        aiAgent.correctionFactor = barrelCorrectionFactor;
                    }
                    
                    break;
                case "Task 9":
                    if (!barrel.GetComponent<Task_9.AI.AIAgent>())
                    {
                        barrel.AddComponent<Task_9.AI.AIAgent>();
                        Task_9.AI.AIAgent aiAgent = barrel.GetComponent<Task_9.AI.AIAgent>();
                        aiAgent.maxSpeed = barrelMaxSpeed;
                        aiAgent.islands = environment;
                    }

                    if (!barrel.GetComponent<Task_9.AI.Wander>())
                    {
                        barrel.AddComponent<Task_9.AI.Wander>();
                        Task_9.AI.Wander aiAgent = barrel.GetComponent<Task_9.AI.Wander>();
                        aiAgent.wanderDegrees = barrelWanderDegree;
                        aiAgent.wanderInterval = barrelWanderInterval;
                        aiAgent.orbitRadius = barrelRadius;
                        aiAgent.correctionFactor = barrelCorrectionFactor;
                    }
                    
                    break;
            }
        }
        
        foreach (GameObject pirate in pirateShips)
        {
            Transform fovTransform;
            GameObject fovObject;
            switch (sceneName)
            {
                case "Task 4":
                    // Attach all behavioural scripts to each pirates
                    if (!pirate.GetComponent<AIAgent>())
                    {
                        pirate.AddComponent<AIAgent>();
                        AIAgent aiAgent = pirate.GetComponent<AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.island = island;
                        aiAgent.maxSpeed = pirateShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.fovDistance = fovDistance;
                        aiAgent.fovAngle = fovAngle;
                        aiAgent.segments = segments;
                    }
                    
                    if (!pirate.GetComponent<Avoidance>())
                    {
                        pirate.AddComponent<Avoidance>();
                        Avoidance aiAgent = pirate.GetComponent<Avoidance>();
                        aiAgent.avoidanceRadius = pirateShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = pirateShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }
                    
                    if (!pirate.GetComponent<FaceDirection>())
                    {
                        pirate.AddComponent<FaceDirection>();
                    }
                    
                    if (!pirate.GetComponent<Pursue>())
                    {
                        pirate.AddComponent<Pursue>();
                    }
                    
                    if (!pirate.GetComponent<Wander>())
                    {
                        pirate.AddComponent<Wander>();
                        Wander aiAgent = pirate.GetComponent<Wander>();
                        aiAgent.wanderDegrees = wanderDegrees;
                        aiAgent.wanderInterval = wanderInterval;
                        aiAgent.orbitRadius = orbitRadius;
                        aiAgent.correctionFactor = correctionFactor;
                    }
                    
                    // Attach all scripts related to the behaviour of the FOV field 
                    fovTransform = pirate.transform.Find("FOV");
                    if (fovTransform == null)
                    {
                        Debug.LogError("No FOV transform found.");
                        continue;
                    }
                    
                    fovObject = fovTransform.gameObject;
                    if (!fovObject.GetComponent<FOVTrigger>())
                    {
                        fovObject.AddComponent<FOVTrigger>();
                        if (!pirate.GetComponent<AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for FOVTrigger.");
                            continue;
                        }
                        FOVTrigger fovTrigger = fovObject.GetComponent<FOVTrigger>();
                        fovTrigger.agent = pirate.GetComponent<AIAgent>();
                        AIAgent pirateAI = pirate.GetComponent<AIAgent>();
                        pirateAI.fovTrigger = fovObject.GetComponent<FOVTrigger>();
                    }
                    
                    if (!fovObject.GetComponent<MeshGenerator>())
                    {
                        fovObject.AddComponent<MeshGenerator>();
                        if (!pirate.GetComponent<AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for MeshGenerator.");
                            continue;
                        }
                        MeshGenerator meshGenerator = fovObject.GetComponent<MeshGenerator>();
                        meshGenerator.agent = pirate.GetComponent<AIAgent>();
                    }
                    break;
                case "Task 5":
                    // Attach all behavioural scripts to each pirates
                    if (!pirate.GetComponent<Task_5.AI.AIAgent>())
                    {
                        pirate.AddComponent<Task_5.AI.AIAgent>();
                        Task_5.AI.AIAgent aiAgent = pirate.GetComponent<Task_5.AI.AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.island = island;
                        aiAgent.maxSpeed = pirateShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.fovDistance = fovDistance;
                        aiAgent.fovAngle = fovAngle;
                        aiAgent.segments = segments;
                    }
                    
                    if (!pirate.GetComponent<Task_5.AI.Avoidance>())
                    {
                        pirate.AddComponent<Task_5.AI.Avoidance>();
                        Task_5.AI.Avoidance aiAgent = pirate.GetComponent<Task_5.AI.Avoidance>();
                        aiAgent.avoidanceRadius = pirateShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = pirateShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }
                    
                    if (!pirate.GetComponent<Task_5.AI.FaceDirection>())
                    {
                        pirate.AddComponent<Task_5.AI.FaceDirection>();
                    }
                    
                    if (!pirate.GetComponent<Task_5.AI.Pursue>())
                    {
                        pirate.AddComponent<Task_5.AI.Pursue>();
                    }
                    
                    if (!pirate.GetComponent<Task_5.AI.Wander>())
                    {
                        pirate.AddComponent<Task_5.AI.Wander>();
                        Task_5.AI.Wander aiAgent = pirate.GetComponent<Task_5.AI.Wander>();
                        aiAgent.wanderDegrees = wanderDegrees;
                        aiAgent.wanderInterval = wanderInterval;
                        aiAgent.orbitRadius = orbitRadius;
                        aiAgent.correctionFactor = correctionFactor;
                    }
                    
                    // Attach all scripts related to the behaviour of the FOV field 
                    fovTransform = pirate.transform.Find("FOV");
                    if (fovTransform == null)
                    {
                        Debug.LogError("No FOV transform found.");
                        continue;
                    }
                    
                    fovObject = fovTransform.gameObject;
                    if (!fovObject.GetComponent<Task_5.AI.FOVTrigger>())
                    {
                        fovObject.AddComponent<Task_5.AI.FOVTrigger>();
                        if (!pirate.GetComponent<Task_5.AI.AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for FOVTrigger.");
                            continue;
                        }
                        Task_5.AI.FOVTrigger fovTrigger = fovObject.GetComponent<Task_5.AI.FOVTrigger>();
                        fovTrigger.agent = pirate.GetComponent<Task_5.AI.AIAgent>();
                        // fovTrigger.agent = pirate.GetComponent<Task_5.AI.AIAgent>();
                        Task_5.AI.AIAgent pirateAI = pirate.GetComponent<Task_5.AI.AIAgent>();
                        pirateAI.fovTrigger = fovObject.GetComponent<Task_5.AI.FOVTrigger>();
                    }
                    
                    if (!fovObject.GetComponent<Task_5.AI.MeshGenerator>())
                    {
                        fovObject.AddComponent<Task_5.AI.MeshGenerator>();
                        if (!pirate.GetComponent<Task_5.AI.AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for MeshGenerator.");
                            continue;
                        }
                        Task_5.AI.MeshGenerator meshGenerator = fovObject.GetComponent<Task_5.AI.MeshGenerator>();
                        meshGenerator.agent = pirate.GetComponent<Task_5.AI.AIAgent>();
                    }
                    break;
                case "Task 6":
                    // Attach all behavioural scripts to each pirates
                    if (!pirate.GetComponent<Task_6.AI.AIAgent>())
                    {
                        pirate.AddComponent<Task_6.AI.AIAgent>();
                        Task_6.AI.AIAgent aiAgent = pirate.GetComponent<Task_6.AI.AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.island = island;
                        aiAgent.maxSpeed = pirateShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.fovDistance = fovDistance;
                        aiAgent.fovAngle = fovAngle;
                        aiAgent.segments = segments;
                    }
                    
                    if (!pirate.GetComponent<Task_6.AI.Avoidance>())
                    {
                        pirate.AddComponent<Task_6.AI.Avoidance>();
                        Task_6.AI.Avoidance aiAgent = pirate.GetComponent<Task_6.AI.Avoidance>();
                        aiAgent.avoidanceRadius = pirateShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = pirateShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }
                    
                    if (!pirate.GetComponent<Task_6.AI.FaceDirection>())
                    {
                        pirate.AddComponent<Task_6.AI.FaceDirection>();
                    }
                    
                    if (!pirate.GetComponent<Task_6.AI.Pursue>())
                    {
                        pirate.AddComponent<Task_6.AI.Pursue>();
                    }
                    
                    if (!pirate.GetComponent<Task_6.AI.Wander>())
                    {
                        pirate.AddComponent<Task_6.AI.Wander>();
                        Task_6.AI.Wander aiAgent = pirate.GetComponent<Task_6.AI.Wander>();
                        aiAgent.wanderDegrees = wanderDegrees;
                        aiAgent.wanderInterval = wanderInterval;
                        aiAgent.orbitRadius = orbitRadius;
                        aiAgent.correctionFactor = correctionFactor;
                    }
                    
                    // Attach all scripts related to the behaviour of the FOV field 
                    fovTransform = pirate.transform.Find("FOV");
                    if (fovTransform == null)
                    {
                        Debug.LogError("No FOV transform found.");
                        continue;
                    }
                    
                    fovObject = fovTransform.gameObject;
                    if (!fovObject.GetComponent<Task_6.AI.FOVTrigger>())
                    {
                        fovObject.AddComponent<Task_6.AI.FOVTrigger>();
                        if (!pirate.GetComponent<Task_6.AI.AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for FOVTrigger.");
                            continue;
                        }
                        Task_6.AI.FOVTrigger fovTrigger = fovObject.GetComponent<Task_6.AI.FOVTrigger>();
                        fovTrigger.agent = pirate.GetComponent<Task_6.AI.AIAgent>();
                        // fovTrigger.agent = pirate.GetComponent<Task_6.AI.AIAgent>();
                        Task_6.AI.AIAgent pirateAI = pirate.GetComponent<Task_6.AI.AIAgent>();
                        pirateAI.fovTrigger = fovObject.GetComponent<Task_6.AI.FOVTrigger>();
                    }
                    
                    if (!fovObject.GetComponent<Task_6.AI.MeshGenerator>())
                    {
                        fovObject.AddComponent<Task_6.AI.MeshGenerator>();
                        if (!pirate.GetComponent<Task_6.AI.AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for MeshGenerator.");
                            continue;
                        }
                        Task_6.AI.MeshGenerator meshGenerator = fovObject.GetComponent<Task_6.AI.MeshGenerator>();
                        meshGenerator.agent = pirate.GetComponent<Task_6.AI.AIAgent>();
                    }
                    break;
                case "Task 7":
                    // Attach all behavioural scripts to each pirates
                    if (!pirate.GetComponent<Task_7.AI.AIAgent>())
                    {
                        pirate.AddComponent<Task_7.AI.AIAgent>();
                        Task_7.AI.AIAgent aiAgent = pirate.GetComponent<Task_7.AI.AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.island = island;
                        aiAgent.maxSpeed = pirateShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.fovDistance = fovDistance;
                        aiAgent.fovAngle = fovAngle;
                        aiAgent.segments = segments;
                    }
                    
                    if (!pirate.GetComponent<Task_7.AI.Avoidance>())
                    {
                        pirate.AddComponent<Task_7.AI.Avoidance>();
                        Task_7.AI.Avoidance aiAgent = pirate.GetComponent<Task_7.AI.Avoidance>();
                        aiAgent.avoidanceRadius = pirateShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = pirateShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }
                    
                    if (!pirate.GetComponent<Task_7.AI.FaceDirection>())
                    {
                        pirate.AddComponent<Task_7.AI.FaceDirection>();
                    }
                    
                    if (!pirate.GetComponent<Task_7.AI.Pursue>())
                    {
                        pirate.AddComponent<Task_7.AI.Pursue>();
                    }
                    
                    if (!pirate.GetComponent<Task_7.AI.Wander>())
                    {
                        pirate.AddComponent<Task_7.AI.Wander>();
                        Task_7.AI.Wander aiAgent = pirate.GetComponent<Task_7.AI.Wander>();
                        aiAgent.wanderDegrees = wanderDegrees;
                        aiAgent.wanderInterval = wanderInterval;
                        aiAgent.orbitRadius = orbitRadius;
                        aiAgent.correctionFactor = correctionFactor;
                    }
                    
                    // Attach all scripts related to the behaviour of the FOV field 
                    fovTransform = pirate.transform.Find("FOV");
                    if (fovTransform == null)
                    {
                        Debug.LogError("No FOV transform found.");
                        continue;
                    }
                    
                    fovObject = fovTransform.gameObject;
                    if (!fovObject.GetComponent<Task_7.AI.FOVTrigger>())
                    {
                        fovObject.AddComponent<Task_7.AI.FOVTrigger>();
                        if (!pirate.GetComponent<Task_7.AI.AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for FOVTrigger.");
                            continue;
                        }
                        Task_7.AI.FOVTrigger fovTrigger = fovObject.GetComponent<Task_7.AI.FOVTrigger>();
                        fovTrigger.agent = pirate.GetComponent<Task_7.AI.AIAgent>();
                        // fovTrigger.agent = pirate.GetComponent<Task_7.AI.AIAgent>();
                        Task_7.AI.AIAgent pirateAI = pirate.GetComponent<Task_7.AI.AIAgent>();
                        pirateAI.fovTrigger = fovObject.GetComponent<Task_7.AI.FOVTrigger>();
                    }
                    
                    if (!fovObject.GetComponent<Task_7.AI.MeshGenerator>())
                    {
                        fovObject.AddComponent<Task_7.AI.MeshGenerator>();
                        if (!pirate.GetComponent<Task_7.AI.AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for MeshGenerator.");
                            continue;
                        }
                        Task_7.AI.MeshGenerator meshGenerator = fovObject.GetComponent<Task_7.AI.MeshGenerator>();
                        meshGenerator.agent = pirate.GetComponent<Task_7.AI.AIAgent>();
                    }
                    break;
                case "Task 8":
                    // Attach all behavioural scripts to each pirates
                    if (!pirate.GetComponent<Task_8.AI.AIAgent>())
                    {
                        pirate.AddComponent<Task_8.AI.AIAgent>();
                        Task_8.AI.AIAgent aiAgent = pirate.GetComponent<Task_8.AI.AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.islands = environment;
                        aiAgent.maxSpeed = pirateShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.fovDistance = fovDistance;
                        aiAgent.fovAngle = fovAngle;
                        aiAgent.segments = segments;
                    }
                    
                    if (!pirate.GetComponent<Task_8.AI.Avoidance>())
                    {
                        pirate.AddComponent<Task_8.AI.Avoidance>();
                        Task_8.AI.Avoidance aiAgent = pirate.GetComponent<Task_8.AI.Avoidance>();
                        aiAgent.avoidanceRadius = pirateShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = pirateShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }
                    
                    if (!pirate.GetComponent<Task_8.AI.FaceDirection>())
                    {
                        pirate.AddComponent<Task_8.AI.FaceDirection>();
                    }
                    
                    if (!pirate.GetComponent<Task_8.AI.Pursue>())
                    {
                        pirate.AddComponent<Task_8.AI.Pursue>();
                    }
                    
                    if (!pirate.GetComponent<Task_8.AI.Wander>())
                    {
                        pirate.AddComponent<Task_8.AI.Wander>();
                        Task_8.AI.Wander aiAgent = pirate.GetComponent<Task_8.AI.Wander>();
                        aiAgent.wanderDegrees = wanderDegrees;
                        aiAgent.wanderInterval = wanderInterval;
                        aiAgent.orbitRadius = orbitRadius;
                        aiAgent.correctionFactor = correctionFactor;
                    }
                    
                    // Attach all scripts related to the behaviour of the FOV field 
                    fovTransform = pirate.transform.Find("FOV");
                    if (fovTransform == null)
                    {
                        Debug.LogError("No FOV transform found.");
                        continue;
                    }
                    
                    fovObject = fovTransform.gameObject;
                    if (!fovObject.GetComponent<Task_8.AI.FOVTrigger>())
                    {
                        fovObject.AddComponent<Task_8.AI.FOVTrigger>();
                        if (!pirate.GetComponent<Task_8.AI.AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for FOVTrigger.");
                            continue;
                        }
                        Task_8.AI.FOVTrigger fovTrigger = fovObject.GetComponent<Task_8.AI.FOVTrigger>();
                        fovTrigger.agent = pirate.GetComponent<Task_8.AI.AIAgent>();
                        // fovTrigger.agent = pirate.GetComponent<Task_8.AI.AIAgent>();
                        Task_8.AI.AIAgent pirateAI = pirate.GetComponent<Task_8.AI.AIAgent>();
                        pirateAI.fovTrigger = fovObject.GetComponent<Task_8.AI.FOVTrigger>();
                    }
                    
                    if (!fovObject.GetComponent<Task_8.AI.MeshGenerator>())
                    {
                        fovObject.AddComponent<Task_8.AI.MeshGenerator>();
                        if (!pirate.GetComponent<Task_8.AI.AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for MeshGenerator.");
                            continue;
                        }
                        Task_8.AI.MeshGenerator meshGenerator = fovObject.GetComponent<Task_8.AI.MeshGenerator>();
                        meshGenerator.agent = pirate.GetComponent<Task_8.AI.AIAgent>();
                    }
                    break;
                case "Task 9":
                    // Attach all behavioural scripts to each pirates
                    if (!pirate.GetComponent<Task_9.AI.AIAgent>())
                    {
                        pirate.AddComponent<Task_9.AI.AIAgent>();
                        Task_9.AI.AIAgent aiAgent = pirate.GetComponent<Task_9.AI.AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.islands = environment;
                        aiAgent.maxSpeed = pirateShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.fovDistance = fovDistance;
                        aiAgent.fovAngle = fovAngle;
                        aiAgent.segments = segments;
                    }
                    
                    if (!pirate.GetComponent<Task_9.AI.Avoidance>())
                    {
                        pirate.AddComponent<Task_9.AI.Avoidance>();
                        Task_9.AI.Avoidance aiAgent = pirate.GetComponent<Task_9.AI.Avoidance>();
                        aiAgent.avoidanceRadius = pirateShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = pirateShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }
                    
                    if (!pirate.GetComponent<Task_9.AI.FaceDirection>())
                    {
                        pirate.AddComponent<Task_9.AI.FaceDirection>();
                    }
                    
                    if (!pirate.GetComponent<Task_9.AI.Pursue>())
                    {
                        pirate.AddComponent<Task_9.AI.Pursue>();
                    }
                    
                    if (!pirate.GetComponent<Task_9.AI.Wander>())
                    {
                        pirate.AddComponent<Task_9.AI.Wander>();
                        Task_9.AI.Wander aiAgent = pirate.GetComponent<Task_9.AI.Wander>();
                        aiAgent.wanderDegrees = wanderDegrees;
                        aiAgent.wanderInterval = wanderInterval;
                        aiAgent.orbitRadius = orbitRadius;
                        aiAgent.correctionFactor = correctionFactor;
                    }
                    
                    // Attach all scripts related to the behaviour of the FOV field 
                    fovTransform = pirate.transform.Find("FOV");
                    if (fovTransform == null)
                    {
                        Debug.LogError("No FOV transform found.");
                        continue;
                    }
                    
                    fovObject = fovTransform.gameObject;
                    if (!fovObject.GetComponent<Task_9.AI.FOVTrigger>())
                    {
                        fovObject.AddComponent<Task_9.AI.FOVTrigger>();
                        if (!pirate.GetComponent<Task_9.AI.AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for FOVTrigger.");
                            continue;
                        }
                        Task_9.AI.FOVTrigger fovTrigger = fovObject.GetComponent<Task_9.AI.FOVTrigger>();
                        fovTrigger.agent = pirate.GetComponent<Task_9.AI.AIAgent>();
                        // fovTrigger.agent = pirate.GetComponent<Task_9.AI.AIAgent>();
                        Task_9.AI.AIAgent pirateAI = pirate.GetComponent<Task_9.AI.AIAgent>();
                        pirateAI.fovTrigger = fovObject.GetComponent<Task_9.AI.FOVTrigger>();
                    }
                    
                    if (!fovObject.GetComponent<Task_9.AI.MeshGenerator>())
                    {
                        fovObject.AddComponent<Task_9.AI.MeshGenerator>();
                        if (!pirate.GetComponent<Task_9.AI.AIAgent>())
                        {
                            Debug.LogError("No AIAgent component found for MeshGenerator.");
                            continue;
                        }
                        Task_9.AI.MeshGenerator meshGenerator = fovObject.GetComponent<Task_9.AI.MeshGenerator>();
                        meshGenerator.agent = pirate.GetComponent<Task_9.AI.AIAgent>();
                    }
                    break;
            }
        }

        foreach (GameObject tradeShip in tradeShips)
        {
            switch (sceneName)
            {
                case "Task 4":
                    if (!tradeShip.GetComponent<AIAgent>())
                    {
                        tradeShip.AddComponent<AIAgent>();
                        AIAgent aiAgent = tradeShip.GetComponent<AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.island = island;
                        aiAgent.maxSpeed = tradeShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.viewDistance = viewDistance;
                    }

                    if (!tradeShip.GetComponent<Arrive>())
                    {
                        tradeShip.AddComponent<Arrive>();
                        Arrive aiAgent = tradeShip.GetComponent<Arrive>();
                        aiAgent.slowRadius = arriveSlowRadius;
                        aiAgent.stopRadius = arriveStopRadius;
                    }
                    if (!tradeShip.GetComponent<Avoidance>())
                    {
                        tradeShip.AddComponent<Avoidance>();
                        Avoidance aiAgent = tradeShip.GetComponent<Avoidance>();
                        aiAgent.avoidanceRadius = tradeShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = tradeShipAvoidanceFactor;
                    }

                    if (!tradeShip.GetComponent<FaceDirection>())
                    {
                        tradeShip.AddComponent<FaceDirection>();
                    }

                    if (!tradeShip.GetComponent<Flee>())
                    {
                        tradeShip.AddComponent<Flee>();
                        Flee aiAgent = tradeShip.GetComponent<Flee>();
                        aiAgent.seekWeight = seekWeight;
                        aiAgent.velocityMatchWeight = velocityMatchWeight;
                    }

                    if (!tradeShip.GetComponent<Sneak>())
                    {
                        tradeShip.AddComponent<Sneak>();
                        Sneak aiAgent = tradeShip.GetComponent<Sneak>();
                        aiAgent.bufferDistance = sneakBufferDistance;
                        aiAgent.slowRadius = sneakSlowRadius;
                        aiAgent.crowdingSeparation = sneakCrowdingSeparation;
                    }
                    break;
                case "Task 5":
                    if (!tradeShip.GetComponent<Task_5.AI.AIAgent>())
                    {
                        tradeShip.AddComponent<Task_5.AI.AIAgent>();
                        Task_5.AI.AIAgent aiAgent = tradeShip.GetComponent<Task_5.AI.AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.island = island;
                        aiAgent.maxSpeed = tradeShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.viewDistance = viewDistance;
                    }

                    if (!tradeShip.GetComponent<Task_5.AI.Arrive>())
                    {
                        tradeShip.AddComponent<Task_5.AI.Arrive>();
                        Task_5.AI.Arrive aiAgent = tradeShip.GetComponent<Task_5.AI.Arrive>();
                        aiAgent.slowRadius = arriveSlowRadius;
                        aiAgent.stopRadius = arriveStopRadius;
                    }
                    
                    if (!tradeShip.GetComponent<Task_5.AI.Avoidance>())
                    {
                        tradeShip.AddComponent<Task_5.AI.Avoidance>();
                        Task_5.AI.Avoidance aiAgent = tradeShip.GetComponent<Task_5.AI.Avoidance>();
                        aiAgent.avoidanceRadius = tradeShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = tradeShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }

                    if (!tradeShip.GetComponent<Task_5.AI.FaceDirection>())
                    {
                        tradeShip.AddComponent<Task_5.AI.FaceDirection>();
                    }

                    if (!tradeShip.GetComponent<Task_5.AI.Flee>())
                    {
                        tradeShip.AddComponent<Task_5.AI.Flee>();
                        Task_5.AI.Flee aiAgent = tradeShip.GetComponent<Task_5.AI.Flee>();
                        aiAgent.seekWeight = seekWeight;
                        aiAgent.velocityMatchWeight = velocityMatchWeight;
                    }

                    if (!tradeShip.GetComponent<Task_5.AI.Sneak>())
                    {
                        tradeShip.AddComponent<Task_5.AI.Sneak>();
                        Task_5.AI.Sneak aiAgent = tradeShip.GetComponent<Task_5.AI.Sneak>();
                        aiAgent.bufferDistance = sneakBufferDistance;
                        aiAgent.slowRadius = sneakSlowRadius;
                        aiAgent.crowdingSeparation = sneakCrowdingSeparation;
                    }

                    if (!tradeShip.GetComponent<Task_5.AI.Seek>())
                    {
                        tradeShip.AddComponent<Task_5.AI.Seek>();
                    }
                    break;
                case "Task 6":
                    if (!tradeShip.GetComponent<Task_6.AI.AIAgent>())
                    {
                        tradeShip.AddComponent<Task_6.AI.AIAgent>();
                        Task_6.AI.AIAgent aiAgent = tradeShip.GetComponent<Task_6.AI.AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.island = island;
                        aiAgent.maxSpeed = tradeShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.viewDistance = viewDistance;
                    }

                    if (!tradeShip.GetComponent<Task_6.AI.Arrive>())
                    {
                        tradeShip.AddComponent<Task_6.AI.Arrive>();
                        Task_6.AI.Arrive aiAgent = tradeShip.GetComponent<Task_6.AI.Arrive>();
                        aiAgent.slowRadius = arriveSlowRadius;
                        aiAgent.stopRadius = arriveStopRadius;
                    }
                    
                    if (!tradeShip.GetComponent<Task_6.AI.Avoidance>())
                    {
                        tradeShip.AddComponent<Task_6.AI.Avoidance>();
                        Task_6.AI.Avoidance aiAgent = tradeShip.GetComponent<Task_6.AI.Avoidance>();
                        aiAgent.avoidanceRadius = tradeShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = tradeShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }

                    if (!tradeShip.GetComponent<Task_6.AI.FaceDirection>())
                    {
                        tradeShip.AddComponent<Task_6.AI.FaceDirection>();
                    }

                    if (!tradeShip.GetComponent<Task_6.AI.Flee>())
                    {
                        tradeShip.AddComponent<Task_6.AI.Flee>();
                        Task_6.AI.Flee aiAgent = tradeShip.GetComponent<Task_6.AI.Flee>();
                        aiAgent.seekWeight = seekWeight;
                        aiAgent.velocityMatchWeight = velocityMatchWeight;
                    }

                    if (!tradeShip.GetComponent<Task_6.AI.Sneak>())
                    {
                        tradeShip.AddComponent<Task_6.AI.Sneak>();
                        Task_6.AI.Sneak aiAgent = tradeShip.GetComponent<Task_6.AI.Sneak>();
                        aiAgent.bufferDistance = sneakBufferDistance;
                        aiAgent.slowRadius = sneakSlowRadius;
                        aiAgent.crowdingSeparation = sneakCrowdingSeparation;
                    }

                    if (!tradeShip.GetComponent<Task_6.AI.Seek>())
                    {
                        tradeShip.AddComponent<Task_6.AI.Seek>();
                    }
                    break;
                case "Task 7":
                    if (!tradeShip.GetComponent<Task_7.AI.AIAgent>())
                    {
                        tradeShip.AddComponent<Task_7.AI.AIAgent>();
                        Task_7.AI.AIAgent aiAgent = tradeShip.GetComponent<Task_7.AI.AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.island = island;
                        aiAgent.maxSpeed = tradeShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.viewDistance = viewDistance;
                    }

                    if (!tradeShip.GetComponent<Task_7.AI.Arrive>())
                    {
                        tradeShip.AddComponent<Task_7.AI.Arrive>();
                        Task_7.AI.Arrive aiAgent = tradeShip.GetComponent<Task_7.AI.Arrive>();
                        aiAgent.slowRadius = arriveSlowRadius;
                        aiAgent.stopRadius = arriveStopRadius;
                    }
                    
                    if (!tradeShip.GetComponent<Task_7.AI.Avoidance>())
                    {
                        tradeShip.AddComponent<Task_7.AI.Avoidance>();
                        Task_7.AI.Avoidance aiAgent = tradeShip.GetComponent<Task_7.AI.Avoidance>();
                        aiAgent.avoidanceRadius = tradeShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = tradeShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }

                    if (!tradeShip.GetComponent<Task_7.AI.FaceDirection>())
                    {
                        tradeShip.AddComponent<Task_7.AI.FaceDirection>();
                    }

                    if (!tradeShip.GetComponent<Task_7.AI.Flee>())
                    {
                        tradeShip.AddComponent<Task_7.AI.Flee>();
                        Task_7.AI.Flee aiAgent = tradeShip.GetComponent<Task_7.AI.Flee>();
                        aiAgent.seekWeight = seekWeight;
                        aiAgent.velocityMatchWeight = velocityMatchWeight;
                    }

                    if (!tradeShip.GetComponent<Task_7.AI.Sneak>())
                    {
                        tradeShip.AddComponent<Task_7.AI.Sneak>();
                        Task_7.AI.Sneak aiAgent = tradeShip.GetComponent<Task_7.AI.Sneak>();
                        aiAgent.bufferDistance = sneakBufferDistance;
                        aiAgent.slowRadius = sneakSlowRadius;
                        aiAgent.crowdingSeparation = sneakCrowdingSeparation;
                    }

                    if (!tradeShip.GetComponent<Task_7.AI.Seek>())
                    {
                        tradeShip.AddComponent<Task_7.AI.Seek>();
                    }
                    break;
                case "Task 8":
                    if (!tradeShip.GetComponent<Task_8.AI.AIAgent>())
                    {
                        tradeShip.AddComponent<Task_8.AI.AIAgent>();
                        Task_8.AI.AIAgent aiAgent = tradeShip.GetComponent<Task_8.AI.AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.islands = environment;
                        aiAgent.maxSpeed = tradeShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.viewDistance = viewDistance;
                    }

                    if (!tradeShip.GetComponent<Task_8.AI.Arrive>())
                    {
                        tradeShip.AddComponent<Task_8.AI.Arrive>();
                        Task_8.AI.Arrive aiAgent = tradeShip.GetComponent<Task_8.AI.Arrive>();
                        aiAgent.slowRadius = arriveSlowRadius;
                        aiAgent.stopRadius = arriveStopRadius;
                    }
                    
                    if (!tradeShip.GetComponent<Task_8.AI.Avoidance>())
                    {
                        tradeShip.AddComponent<Task_8.AI.Avoidance>();
                        Task_8.AI.Avoidance aiAgent = tradeShip.GetComponent<Task_8.AI.Avoidance>();
                        aiAgent.avoidanceRadius = tradeShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = tradeShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }

                    if (!tradeShip.GetComponent<Task_8.AI.FaceDirection>())
                    {
                        tradeShip.AddComponent<Task_8.AI.FaceDirection>();
                    }

                    if (!tradeShip.GetComponent<Task_8.AI.Flee>())
                    {
                        tradeShip.AddComponent<Task_8.AI.Flee>();
                        Task_8.AI.Flee aiAgent = tradeShip.GetComponent<Task_8.AI.Flee>();
                        aiAgent.seekWeight = seekWeight;
                        aiAgent.velocityMatchWeight = velocityMatchWeight;
                    }

                    if (!tradeShip.GetComponent<Task_8.AI.Sneak>())
                    {
                        tradeShip.AddComponent<Task_8.AI.Sneak>();
                        Task_8.AI.Sneak aiAgent = tradeShip.GetComponent<Task_8.AI.Sneak>();
                        aiAgent.bufferDistance = sneakBufferDistance;
                        aiAgent.slowRadius = sneakSlowRadius;
                        aiAgent.crowdingSeparation = sneakCrowdingSeparation;
                    }

                    if (!tradeShip.GetComponent<Task_8.AI.Seek>())
                    {
                        tradeShip.AddComponent<Task_8.AI.Seek>();
                    }
                    break;
                case "Task 9":
                    if (!tradeShip.GetComponent<Task_9.AI.AIAgent>())
                    {
                        tradeShip.AddComponent<Task_9.AI.AIAgent>();
                        Task_9.AI.AIAgent aiAgent = tradeShip.GetComponent<Task_9.AI.AIAgent>();
                        aiAgent.ports = harbors;
                        aiAgent.islands = environment;
                        aiAgent.maxSpeed = tradeShipMaxSpeed;
                        aiAgent.lockY = lockY;
                        aiAgent.viewDistance = viewDistance;
                    }

                    if (!tradeShip.GetComponent<Task_9.AI.Arrive>())
                    {
                        tradeShip.AddComponent<Task_9.AI.Arrive>();
                        Task_9.AI.Arrive aiAgent = tradeShip.GetComponent<Task_9.AI.Arrive>();
                        aiAgent.slowRadius = arriveSlowRadius;
                        aiAgent.stopRadius = arriveStopRadius;
                    }
                    
                    if (!tradeShip.GetComponent<Task_9.AI.Avoidance>())
                    {
                        tradeShip.AddComponent<Task_9.AI.Avoidance>();
                        Task_9.AI.Avoidance aiAgent = tradeShip.GetComponent<Task_9.AI.Avoidance>();
                        aiAgent.avoidanceRadius = tradeShipAvoidanceRadius;
                        aiAgent.avoidanceFactor = tradeShipAvoidanceFactor;
                        aiAgent.debug = debug;
                    }

                    if (!tradeShip.GetComponent<Task_9.AI.FaceDirection>())
                    {
                        tradeShip.AddComponent<Task_9.AI.FaceDirection>();
                    }

                    if (!tradeShip.GetComponent<Task_9.AI.Flee>())
                    {
                        tradeShip.AddComponent<Task_9.AI.Flee>();
                        Task_9.AI.Flee aiAgent = tradeShip.GetComponent<Task_9.AI.Flee>();
                        aiAgent.seekWeight = seekWeight;
                        aiAgent.velocityMatchWeight = velocityMatchWeight;
                    }

                    if (!tradeShip.GetComponent<Task_9.AI.Sneak>())
                    {
                        tradeShip.AddComponent<Task_9.AI.Sneak>();
                        Task_9.AI.Sneak aiAgent = tradeShip.GetComponent<Task_9.AI.Sneak>();
                        aiAgent.bufferDistance = sneakBufferDistance;
                        aiAgent.slowRadius = sneakSlowRadius;
                        aiAgent.crowdingSeparation = sneakCrowdingSeparation;
                    }

                    if (!tradeShip.GetComponent<Task_9.AI.Seek>())
                    {
                        tradeShip.AddComponent<Task_9.AI.Seek>();
                    }
                    break;
            }
        }
    }
}