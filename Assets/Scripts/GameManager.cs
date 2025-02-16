using System.Collections.Generic;
using Task_4.AI;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private List<Material> materials;
    [SerializeField] private GameObject tradeShipPrefab;
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
        return sceneName == "MainMenu" || sceneName == "Task 4" || sceneName == "Task 5" || sceneName == "Task 6" || sceneName == "Task 7";
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
        
    public void ClearPirates()
    {
        pirateShips.Clear();
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
            // Debug.LogError(tradeShip.name);
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
       
        
        pirateShips.AddRange(GameObject.FindGameObjectsWithTag("PirateShip"));
        harbors.AddRange(GameObject.FindGameObjectsWithTag("Port"));
        barrels.AddRange(GameObject.FindGameObjectsWithTag("Barrel"));
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
        float angle = Random.Range(0f, 360f);
        float radians = angle * Mathf.Deg2Rad;
        
        float x = Mathf.Cos(radians) * radius;
        float z = Mathf.Sin(radians) * radius;
        
        return new Vector3(harborPosition.x + x, harborPosition.y, harborPosition.z + z);
    }

    private void ApplyScripts(string sceneName)
    {
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
            }
        }
    }
}