using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Task_8.AI
{
    public class AIAgent : MonoBehaviour
    {
        public enum ShipState
        {
            Idle, Navigating, Docking, Waiting, Undocking, Wandering, Pursuing, Fleeing, Sneaking, TargetLost, Avoiding, Seeking, Pathing, Fishing, Fished
        }

        public ShipState CurrentState { get; private set; } = ShipState.Idle;

        public void SetState(ShipState newState) {
            CurrentState = newState;
        }
        
        private Queue<Transform> _portQueue = new Queue<Transform>();
        
        [Header("Fishing Ship Stuff")]
        private Node[] _nodes;
        private int _nodeIndex;
        private Node _currentNode;
        private Node _lastNode;
        
        
        [Header("Other Objects")]
        public List<GameObject> ports;
        public List<GameObject> islands;
        public FOVTrigger fovTrigger;
        public Queue<GameObject> Obstacles = new Queue<GameObject>();
        private Transform _latestTarget;
        [FormerlySerializedAs("_latestPort")] public Transform latestPort;
        public List<Transform> pursuingPirates = new List<Transform>();
        
        
        [Header("Agent Settings")]
        public float maxSpeed;
        public bool lockY = true;
        public bool debug;
        public float viewDistance;
        public float fovDistance;
        public float fovAngle;
        public int segments;
        public Vector3 Velocity { get; set; }
        
        [Header("Target Information")]
        [SerializeField] private Transform trackedTarget;
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Vector3 targetVelocity;
        [SerializeField] private float targetMaxSpeed;
        private Transform _sneakTarget;
        
        public Transform TrackedTarget
        {
            get => trackedTarget;
        }

        public float TargetMaxSpeed
        {
            get => targetMaxSpeed;
        }
        
        public Vector3 TargetPosition
        {
            get => trackedTarget != null ? trackedTarget.position : targetPosition;
        }

        public Vector3 TargetVelocity
        {
            get => trackedTarget != null ? trackedTarget.GetComponent<AIAgent>().Velocity : targetVelocity;
        }

        private void Start()
        {
            if (transform.CompareTag("Barrel"))
            {
                
            }
            else if (transform.CompareTag("FishingShip"))
            {
                foreach (GameObject port in ports)
                {
                    _portQueue.Enqueue(port.GetComponent<Transform>());
                }
                _nodes = GameManager.Instance.GetPath().Nodes.ToArray();
                _nodeIndex = 0;
                SetRandomStartNode();
                CurrentState = ShipState.Pathing;
            }
            else if (ports.Count > 0 && transform.CompareTag("TradeShip"))
            {
                foreach (GameObject port in ports)
                {
                    _portQueue.Enqueue(port.GetComponent<Transform>());
                }

                SetNewTarget();
                CurrentState = ShipState.Navigating;
            }
            else if (transform.CompareTag("PirateShip"))
            {
                CurrentState = ShipState.Wandering;
            }
        }

        private void Update()
        {
            if (debug)
            {
                Debug.DrawRay(transform.position, Velocity, Color.green);
            }

            if (transform.CompareTag("Barrel"))
            {
                Move();
            }

            if (transform.CompareTag("FishingShip"))
            {
                foreach (GameObject ship in GameManager.Instance.GetFishingShips())
                {
                    if (ship.transform.Equals(transform))
                    {
                        continue;
                    }
                    Obstacles.Enqueue(ship);
                }
                foreach (GameObject barrel in GameManager.Instance.GetBarrels())
                {
                    Obstacles.Enqueue(barrel);
                }
                foreach (GameObject port in ports)
                {
                    if (port.transform.Equals(trackedTarget))
                    {
                        continue;
                    }

                    Obstacles.Enqueue(port);
                }
                
                if (CurrentState == ShipState.Pathing)
                {
                    foreach (GameObject island in islands)
                    {
                        Obstacles.Enqueue(island);
                    }
                    
                    Move();
                    
                }

                if (CurrentState == ShipState.Fishing)
                {
                    StartCoroutine(FishingSequence());
                }

                if (CurrentState == ShipState.Fished)
                {
                    trackedTarget = _currentNode.transform;
                    CurrentState = ShipState.Pathing;
                }
                Obstacles.Clear();
            }

            // Trade ship behaviour
            if (transform.CompareTag("TradeShip"))
            {
                UpdateGoals();
                foreach (GameObject barrel in GameManager.Instance.GetBarrels())
                {
                    Obstacles.Enqueue(barrel);
                }
                
                if (debug)
                {
                    // Debug.Log(CurrentState);
                    DebugUtils.DrawCircle(transform.position, transform.up, Color.black, viewDistance);
                }
                
                if (PursuitRegistry.Instance.IsPursued(transform) && CurrentState != ShipState.Fleeing)
                {
                    trackedTarget = GetClosestHarbor(transform, _portQueue);
                    CurrentState = ShipState.Fleeing;
                }
                
                if (CurrentState == ShipState.Navigating)
                {
                    // Check if you can sneak first
                    _sneakTarget = GetSneakTarget();
                    if (_sneakTarget != null)
                    {
                        trackedTarget = _sneakTarget;
                        CurrentState = ShipState.Sneaking;
                    }
                    
                    foreach (GameObject island in islands)
                    {
                        Obstacles.Enqueue(island);
                    }
                    
                    foreach (GameObject ship in GameManager.Instance.GetTradeShips())
                    {
                        if (ship.transform.Equals(transform))
                        {
                            continue;
                        }

                        Obstacles.Enqueue(ship);
                    }
                    foreach (GameObject barrel in GameManager.Instance.GetBarrels())
                    {
                        Obstacles.Enqueue(barrel);
                    }
                    foreach (GameObject ship in GameManager.Instance.GetFishingShips())
                    {
                        Obstacles.Enqueue(ship);
                    }
                    foreach (GameObject pirate in GameManager.Instance.GetPirates())
                    {
                        Obstacles.Enqueue(pirate);
                    }
                    foreach (GameObject port in ports)
                    {
                        if (port.transform.Equals(trackedTarget))
                        {
                            continue;
                        }

                        Obstacles.Enqueue(port);
                    }
                    Move();
                    Obstacles.Clear();
                }

                if (CurrentState == ShipState.Docking)
                {
                    StartCoroutine(DockingSquence());
                }

                if (CurrentState == ShipState.Undocking)
                {
                    SetNewTarget();
                    CurrentState = ShipState.Navigating;
                }

                if (CurrentState == ShipState.Fleeing)
                {
                    foreach (GameObject island in islands)
                    {
                        Obstacles.Enqueue(island);
                    }
                    
                    foreach (GameObject ship in GameManager.Instance.GetTradeShips())
                    {
                        if (ship.transform.Equals(transform))
                        {
                            continue;
                        }

                        Obstacles.Enqueue(ship);
                    }
                    foreach (GameObject barrel in GameManager.Instance.GetBarrels())
                    {
                        Obstacles.Enqueue(barrel);
                    }
                    foreach (GameObject ship in GameManager.Instance.GetFishingShips())
                    {
                        Obstacles.Enqueue(ship);
                    }
                    foreach (GameObject port in ports)
                    {
                        if (port.transform.Equals(trackedTarget))
                        {
                            continue;
                        }

                        Obstacles.Enqueue(port);
                    }
                    pursuingPirates = PursuitRegistry.Instance.GetPursuingShip(transform);
                    if (pursuingPirates.Count > 0)
                    {
                        Move();   
                    }
                    else
                    {
                        trackedTarget = latestPort;
                        CurrentState = ShipState.Navigating;
                    }
                    Obstacles.Clear();
                }

                if (CurrentState == ShipState.Sneaking)
                {
                    // Check first if it's still worth it to flee
                    _sneakTarget = GetSneakTarget();
                    if (_sneakTarget != null)
                    {
                        if (trackedTarget.CompareTag("Port"))
                        {
                            Debug.Log("Somehow Entered");
                            _latestTarget = trackedTarget;
                            Debug.Log(_latestTarget);
                        }
                        trackedTarget = _sneakTarget;
                        foreach (GameObject island in islands)
                        {
                            Obstacles.Enqueue(island);
                        }
                        foreach (GameObject barrel in GameManager.Instance.GetBarrels())
                        {
                            Obstacles.Enqueue(barrel);
                        }
                        foreach (GameObject ship in GameManager.Instance.GetTradeShips())
                        {
                            if (ship.transform.Equals(transform))
                            {
                                continue;
                            }

                            Obstacles.Enqueue(ship);
                        }
                        foreach (GameObject port in ports)
                        {
                            if (port.transform.Equals(trackedTarget))
                            {
                                continue;
                            }

                            Obstacles.Enqueue(port);
                        }
                        Move();
                        Obstacles.Clear();
                    }
                    else
                    {
                        trackedTarget = latestPort;
                        CurrentState = ShipState.Navigating;
                    }
                }

                if (CurrentState == ShipState.Seeking)
                {
                    // Debug.Log("Seeking");
                    foreach (GameObject island in islands)
                    {
                        Obstacles.Enqueue(island);
                    }
                    foreach (GameObject barrel in GameManager.Instance.GetBarrels())
                    {
                        Obstacles.Enqueue(barrel);
                    }
                    foreach (GameObject ship in GameManager.Instance.GetTradeShips())
                    {
                        if (ship.transform.Equals(transform))
                        {
                            continue;
                        }

                        Obstacles.Enqueue(ship);
                    }
                    foreach (GameObject ship in GameManager.Instance.GetFishingShips())
                    {
                        Obstacles.Enqueue(ship);
                    }
                    foreach (GameObject pirate in GameManager.Instance.GetPirates())
                    {
                        // if (pirate.transform.Equals(trackedTarget))
                        // {
                        //     continue;
                        // }

                        Obstacles.Enqueue(pirate);
                    }
                    trackedTarget = GetClosestPirate();
                    Move();
                    Obstacles.Clear();
                }
            }

            // Pirate ship behaviour
            if (transform.CompareTag("PirateShip"))
            {
                foreach (GameObject barrel in GameManager.Instance.GetBarrels())
                {
                    Obstacles.Enqueue(barrel);
                }
                
                foreach (GameObject ship in GameManager.Instance.GetFishingShips())
                {
                    Obstacles.Enqueue(ship);
                }
                
                if (fovTrigger.GetClosestTradeShip(transform) != null)
                {
                    if (GameManager.Instance.GetTradeShips().Contains(fovTrigger.GetClosestTradeShip(transform).gameObject))
                    {
                        CurrentState = ShipState.Pursuing;
                        // Debug.Log(fovTrigger.GetClosestTradeShip(transform).gameObject.name);
                        // Debug.Log(GameManager.Instance.GetTradeShips().Count);
                    }
                    else
                    {
                        CurrentState = ShipState.TargetLost;
                    }
                }
                
                if (CurrentState == ShipState.Wandering)
                {
                    foreach (GameObject island in islands)
                    {
                        Obstacles.Enqueue(island);
                    }
                    
                    foreach (GameObject port in ports)
                    {
                        Obstacles.Enqueue(port);
                    }

                    foreach (GameObject pirate in GameManager.Instance.GetPirates())
                    {
                        if (pirate.transform.Equals(transform))
                        {
                            continue;
                        }
                        Obstacles.Enqueue(pirate);
                    }
                    Move();

                    Obstacles.Clear();
                }

                if (CurrentState == ShipState.Pursuing)
                {
                    foreach (GameObject island in islands)
                    {
                        Obstacles.Enqueue(island);
                    }
                    
                    Transform pursuedShip = fovTrigger.GetClosestTradeShip(transform);
                    
                    PursuitRegistry.Instance.RegisterPursuit(transform, pursuedShip);
                    trackedTarget = pursuedShip;
                
                    Move();
                
                    Obstacles.Clear();
                }
            
                if (CurrentState == ShipState.TargetLost)
                {
                    trackedTarget = null;
                    PursuitRegistry.Instance.RemovePursuit(transform);
                    fovTrigger.ClearTrigger();
                    CurrentState = ShipState.Wandering;
                }

                if (CurrentState == ShipState.Avoiding)
                {
                    foreach (GameObject island in islands)
                    {
                        Obstacles.Enqueue(island);
                    }
                    
                    foreach (GameObject port in ports)
                    {
                        Obstacles.Enqueue(port);
                    }

                    foreach (GameObject pirate in GameManager.Instance.GetPirates())
                    {
                        if (pirate.transform.Equals(transform))
                        {
                            continue;
                        }
                        Obstacles.Enqueue(pirate);
                    }
                    Move();
                    Obstacles.Clear();
                }
            }            
        }

        IEnumerator FishingSequence()
        {
            CurrentState = ShipState.Waiting;
            _nodeIndex = (_nodeIndex + 1) % _nodes.Length;
            _lastNode = _currentNode;
            _currentNode = _nodes[_nodeIndex];
            trackedTarget = GetClosestHarbor(transform, _portQueue);
            float waitTime = Random.Range(10f, 15f);
            yield return new WaitForSeconds(waitTime);
            CurrentState = ShipState.Pathing;
        }

        private void UpdateGoals()
        {
            int totalPirates = GameManager.Instance.GetTotalPirates();
            int alivePirates = GameManager.Instance.GetAlivePirates();

            float gameDuration = GameManager.Instance.GetTotalTime();
            float remainingTime = GameManager.Instance.GetRemainingTime();
            
            float threatScore = ((float) alivePirates / totalPirates) / (remainingTime / gameDuration);
            // Debug.Log(threatScore);

            float threatThreshold = 1.5f;
            
            // Verify if you should switch objective
            if (threatScore > threatThreshold)
            {
                // Random chance you might not even if you should
                if (Random.value < 0.8f && (CurrentState == ShipState.Navigating))
                {
                    // You should switch goals from trading to actively trying to get rid of pirates
                    CurrentState = ShipState.Seeking;
                    // Debug.Log("Would have swapped objective");
                }
            }
        }

        private void SetRandomStartNode()
        {
            _nodeIndex = Random.Range(0, _nodes.Length);
            _currentNode = _nodes[_nodeIndex];
            _lastNode = _currentNode;
            trackedTarget = _currentNode.transform;
        }

        private Transform GetClosestPirate()
        {
            Transform closestPirate = null;
            float closestDistanceSqr = float.MaxValue;

            foreach (GameObject pirate in GameManager.Instance.GetPirates())
            {
                float distanceSqr = (pirate.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestPirate = pirate.transform;
                }
            }

            if (closestPirate != null)
            {
                return closestPirate.Find("FOV").transform;
            }
            else
            {
                Debug.LogError("No pirate found");
                return null;
            }
            
        }
        
        private Transform GetSneakTarget()
        {
            Transform bestPirate = null;
            float closestDistance = float.MaxValue;

            foreach (GameObject pirate in GameManager.Instance.GetPirates())
            {
                Transform pirateTransform = pirate.transform;

                Vector3 tradeToTarget = latestPort.position - transform.position;
                Vector3 tradeToPirate = pirateTransform.position - transform.position;
                Vector3 pirateToTarget = latestPort.position - pirateTransform.position;

                float tradeToTargetDist = tradeToTarget.magnitude;
                float tradeToPirateDist = tradeToPirate.magnitude;

                // 1. Check if pirate is within the view radius of the trade ship
                if (tradeToPirateDist > viewDistance)
                {
                    // Debug.Log("Outside of view");
                    continue;
                }
                // Debug.Log("Still in view");
                
                // 2. Check if pirate is physically between trade ship and the target
                float pirateToTargetDist = Vector3.Distance(pirateTransform.position, TargetPosition);
                if (pirateToTargetDist > tradeToTargetDist)
                {
                    // Debug.Log("Not between target");
                    continue;
                } 

                // 3. Check if the angle between trade-to-target and trade-to-pirate exceeds threshold
                float angle = Vector3.Angle(tradeToTarget, pirateToTarget);
                Debug.DrawRay(transform.position, tradeToTarget, Color.cyan);
                Debug.DrawRay(pirateTransform.position, pirateToTarget, Color.blue);
                if (Mathf.Abs(angle) > 10f)
                {
                    // Debug.Log("Too far apart");
                    continue;
                } 

                // Choose the closest pirate that meets criteria
                if (tradeToPirateDist < closestDistance)
                {
                    closestDistance = tradeToPirateDist;
                    bestPirate = pirateTransform;
                }
            }

            return bestPirate;
        }
        
        private Transform GetClosestHarbor(Transform tradeShip, Queue<Transform> harbors)
        {
            Transform closestHarbor = null;
            float closestDistanceSqr = float.MaxValue;

            foreach (Transform harbor in harbors)
            {
                float distanceSqr = (harbor.position - tradeShip.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestHarbor = harbor;
                }
            }

            return closestHarbor;
        }

        private void Move()
        {
            Vector3 steeringForceSum;
            Quaternion rotation;
            GetSteeringSum(out steeringForceSum, out rotation);
            Velocity += steeringForceSum * Time.deltaTime;
            Velocity = Vector3.ClampMagnitude(Velocity, maxSpeed);
            transform.position += Velocity * Time.deltaTime;
            transform.rotation *= rotation;
        }

        IEnumerator DockingSquence()
        {
            CurrentState = ShipState.Waiting;
            yield return new WaitForSeconds(2f);
            CurrentState = ShipState.Undocking;
        }

        private void SetNewTarget()
        {
            Transform newTarget = _portQueue.Dequeue();
            trackedTarget = newTarget;
            latestPort = newTarget;
            _portQueue.Enqueue(newTarget);
        }

        private void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation)
        {
            steeringForceSum = Vector3.zero;
            rotation = Quaternion.identity;
            AIMovement[] movements = GetComponents<AIMovement>();

            if (CurrentState == ShipState.Navigating)
            {
                movements = movements.Where(m => m is Arrive || m is FaceDirection || m is Avoidance).ToArray();
            }
            
            if (CurrentState == ShipState.Fleeing)
            {
                movements = movements.Where(m => m is Flee || m is FaceDirection || m is Avoidance).ToArray();
            }

            if (CurrentState == ShipState.Sneaking)
            {
                movements = movements.Where(m => m is Sneak || m is FaceDirection || m is Avoidance).ToArray();
            }

            if (CurrentState == ShipState.Seeking)
            {
                movements = movements.Where(m => m is Seek || m is FaceDirection || m is Avoidance).ToArray();
            }
            
            if (CurrentState == ShipState.Wandering)
            {
                movements = movements.Where(m => m is Wander || m is FaceDirection || m is Avoidance).ToArray();
            }

            if (CurrentState == ShipState.Pursuing)
            {
                movements = movements.Where(m => m is Pursue || m is FaceDirection || m is Avoidance).ToArray();
            }

            if (CurrentState == ShipState.Pathing)
            {
                movements = movements.Where(m => m is PathFind || m is FaceDirection || m is Avoidance).ToArray();
            }
            
            foreach (AIMovement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).Linear;
                rotation *= movement.GetSteering(this).Angular;
            }
        }
    }
}