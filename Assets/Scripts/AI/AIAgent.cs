using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI
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
        
        private Queue<Transform> _shipQueue = new Queue<Transform>();
        
        [Header("Other Objects")]
        public GameObject[] ports;
        public GameObject[] pirates;
        public FOVTrigger fovTrigger;
        
        [Header("Agent Settings")]
        public float maxSpeed;
        public bool lockY = true;
        public bool debug;
        public float viewDistance;
        public float fovAngle;
        public int segments;
        
        public Vector3 Velocity { get; set; }
        
        [Header("Target Information")]
        [SerializeField] private Transform trackedTarget;
        [SerializeField] private Vector3 targetPosition;
        public Vector3 TargetPosition
        {
            get => trackedTarget != null ? trackedTarget.position : targetPosition;
        }

        private void Start()
        {
            if (ports.Length > 0 && transform.CompareTag("TradeShip"))
            {
                foreach (GameObject port in ports)
                {
                    _shipQueue.Enqueue(port.GetComponent<Transform>());
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

            if (CurrentState == ShipState.Navigating)
            {
                Move();
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

            if (CurrentState == ShipState.Wandering)
            {
                Move();
            }

            if (CurrentState == ShipState.Pursuing)
            {
                Transform pursuedShip = fovTrigger.GetClosestTradeShip(transform);
                trackedTarget = pursuedShip;
                Move();
            }
            
            if (CurrentState == ShipState.TargetLost)
            {
                trackedTarget = null;
                CurrentState = ShipState.Wandering;
            }
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
            Transform newTarget = _shipQueue.Dequeue();
            trackedTarget = newTarget;
            _shipQueue.Enqueue(newTarget);
        }

        private void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation)
        {
            steeringForceSum = Vector3.zero;
            rotation = Quaternion.identity;
            AIMovement[] movements = GetComponents<AIMovement>();

            // If the state is Wandering, only keep the Wander movement
            if (CurrentState == ShipState.Wandering)
            {
                movements = movements.Where(m => m is Wander || m is FaceDirection).ToArray();
            }

            if (CurrentState == ShipState.Pursuing)
            {
                // Debug.Log(movements[0]);
                movements = movements.Where(m => m is Pursue || m is FaceDirection).ToArray();
                // Debug.Log("After: " +movements.Length);
            }
            
            foreach (AIMovement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).Linear;
                rotation *= movement.GetSteering(this).Angular;
            }
        }
    }
}