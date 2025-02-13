using System.Collections.Generic;
using Task_4.AI;
using Unity.VisualScripting;
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
    
    [Header("Objects in Scene")]
    [SerializeField] private List<GameObject> pirateShips = new List<GameObject>();
    [SerializeField] private List<GameObject> tradeShips = new List<GameObject>();
    [SerializeField] private List<GameObject> harbors = new List<GameObject>();
    [SerializeField] private GameObject island;
    
    [Header("General Ship Settings")]
    [SerializeField] private bool lockY;
    [SerializeField] private bool debug;
    

    private float gameTimer = 0f;
    private bool gameOver = false;

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
        if (ShouldPersistAcrossScenes(SceneManager.GetActiveScene().name))
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    
    private bool ShouldPersistAcrossScenes(string sceneName)
    {
        // Define scenes where GameManager should persist
        return sceneName == "MainMenu" || sceneName == "Task 4";
    }

    private void Update()
    {
        if (!gameOver)
        {
            gameTimer += Time.deltaTime;
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
        
    private void CheckGameOver()
    {
        if (tradeShips.Count == 0)
        {
            GameOver("Pirates Win! All trade ships are destroyed.");
        }
        else if (gameTimer >= 300f) 
        {
            GameOver("Time's up! Game over.");
        }
    }
        
    private void GameOver(string message)
    {
        gameOver = true;
        Debug.Log(message);
    }

    private void FetchShips()
    {
        pirateShips.Clear();
        tradeShips.Clear();
        harbors.Clear();
        
        pirateShips.AddRange(GameObject.FindGameObjectsWithTag("PirateShip"));
        tradeShips.AddRange(GameObject.FindGameObjectsWithTag("TradeShip"));
        harbors.AddRange(GameObject.FindGameObjectsWithTag("Port"));
        island = GameObject.FindGameObjectWithTag("Island");
        
        string sceneName = SceneManager.GetActiveScene().name;

        ApplyScripts(sceneName);
    }

    private void ApplyScripts(string sceneName)
    {
        foreach (GameObject pirate in pirateShips)
        {
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
                    Transform fovTransform = pirate.transform.Find("FOV");
                    if (fovTransform == null)
                    {
                        Debug.LogError("No FOV transform found.");
                        continue;
                    }
                    
                    GameObject fovObject = fovTransform.gameObject;
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
                    AIAgent pirateAI = pirate.GetComponent<AIAgent>();
                    pirateAI.fovTrigger = fovObject.GetComponent<FOVTrigger>();
                    
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
            }
        }
    }
}