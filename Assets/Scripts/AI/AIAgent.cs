using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class AIAgent : MonoBehaviour
    {
        public enum ShipState
        {
            Idle, Navigating, Docking, Waiting, Undocking, Wandering, Pursuing
        }

        public ShipState CurrentState { get; private set; } = ShipState.Idle;

        public void SetState(ShipState newState) {
            CurrentState = newState;
            // Debug.Log("Ship state changed to: " + CurrentState);
        }
        
        private Queue<Transform> shipQueue = new Queue<Transform>();
        public GameObject[] portList;
        public float maxSpeed;
        public bool lockY = true;
        public bool debug;
        
        public Vector3 Velocity { get; set; }
        

        [SerializeField] private Transform trackedTarget;
        [SerializeField] private Vector3 targetPosition;
        public Vector3 TargetPosition
        {
            get => trackedTarget != null ? trackedTarget.position : targetPosition;
        }

        private void Start()
        {
            if (portList.Length > 0)
            {
                foreach (GameObject port in portList)
                {
                    shipQueue.Enqueue(port.GetComponent<Transform>());
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
            Transform newTarget = shipQueue.Dequeue();
            trackedTarget = newTarget;
            shipQueue.Enqueue(newTarget);
        }

        private void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation)
        {
            steeringForceSum = Vector3.zero;
            rotation = Quaternion.identity;
            AIMovement[] movements = GetComponents<AIMovement>();
            foreach (AIMovement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).linear;
                rotation *= movement.GetSteering(this).angular;
            }
        }
    }
}