using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Task_3.AI
{
    public class AIAgent : MonoBehaviour
    {
        public enum ShipState
        {
            Idle, Navigating, Docking, Waiting, Undocking, Wandering, Pursuing, Fleeing, Sneaking, TargetLost
        }

        public ShipState CurrentState { get; private set; } = ShipState.Idle;

        public void SetState(ShipState newState) {
            CurrentState = newState;
            // Debug.Log("Ship state changed to: " + CurrentState);
        }
        
        private Queue<Transform> _portQueue = new Queue<Transform>();
        
        [Header("Other Objects")]
        public GameObject[] ports;
        public GameObject[] pirates;
        public GameObject[] tradeShips;
        public GameObject island;
        public FOVTrigger fovTrigger;
        public Queue<GameObject> Obstacles = new Queue<GameObject>();
        private Transform _latestTarget;
        private Transform _latestPort;
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
            if (ports.Length > 0 && transform.CompareTag("TradeShip"))
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
                    foreach (GameObject pirate in pirates)
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
                        trackedTarget = _latestPort;
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
                        trackedTarget = _latestPort;
                        CurrentState = ShipState.Navigating;
                    }
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
            }            
        }
        
        private Transform GetSneakTarget()
        {
            Transform bestPirate = null;
            float closestDistance = float.MaxValue;

            foreach (GameObject pirate in pirates)
            {
                Transform pirateTransform = pirate.transform;

                Vector3 tradeToTarget = _latestPort.position - transform.position;
                Vector3 tradeToPirate = pirateTransform.position - transform.position;
                Vector3 pirateToTarget = _latestPort.position - pirateTransform.position;

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
            _latestPort = newTarget;
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
            
            if (CurrentState == ShipState.Wandering)
            {
                movements = movements.Where(m => m is Wander || m is FaceDirection || m is Avoidance).ToArray();
            }

            if (CurrentState == ShipState.Pursuing)
            {
                movements = movements.Where(m => m is Pursue || m is FaceDirection || m is Avoidance).ToArray();
            }
            
            foreach (AIMovement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).Linear;
                rotation *= movement.GetSteering(this).Angular;
            }
        }
    }
}