using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Task_6.AI
{
    public class AIAgent : MonoBehaviour
    {
        public enum ShipState
        {
            Idle, Navigating, Docking, Waiting, Undocking, Wandering, Pursuing, Fleeing, Sneaking, TargetLost, Avoiding, Seeking
        }

        public ShipState CurrentState { get; private set; } = ShipState.Idle;

        public void SetState(ShipState newState) {
            CurrentState = newState;
        }
        
        private Queue<Transform> _portQueue = new Queue<Transform>();
        
        [Header("Other Objects")]
        public List<GameObject> ports;
        public GameObject island;
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
            if (ports.Count > 0 && transform.CompareTag("TradeShip"))
            {
                foreach (GameObject port in ports)
                {
                    _portQueue.Enqueue(port.GetComponent<Transform>());
                }

                SetNewTarget();
                CurrentState = ShipState.Navigating;
            }
            else
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

            // Trade ship behaviour
            if (transform.CompareTag("TradeShip"))
            {
                UpdateGoals();
                
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
                    
                    Obstacles.Enqueue(island);
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
                    Obstacles.Enqueue(island);
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
                        Obstacles.Enqueue(island);
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
                    Obstacles.Enqueue(island);
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
                if (CurrentState == ShipState.Wandering)
                {
                    Obstacles.Enqueue(island);
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
                    Obstacles.Enqueue(island);

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
                    CurrentState = ShipState.Wandering;
                }

                if (CurrentState == ShipState.Avoiding)
                {
                    Obstacles.Enqueue(island);
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

            // if (CurrentState == ShipState.Avoiding)
            // {
            //     movements = movements.Where(m => m is Avoidance || m is FaceDirection).ToArray();
            // }
            //
            foreach (AIMovement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).Linear;
                rotation *= movement.GetSteering(this).Angular;
            }
        }
    }
}