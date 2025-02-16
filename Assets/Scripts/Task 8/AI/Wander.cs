
using UnityEngine;

namespace Task_8.AI
{
    public class Wander : AIMovement
    {
        [Header("Wander Settings")]
        public float wanderDegrees;
        [Min(0)] public float wanderInterval;
        private float _wanderTimer = 0;

        private Vector3 _lastDirection;
        private Vector3 _lastMovement;

        [Header("Island Settings")]
        private Transform _island;
        public float orbitRadius;
        public float correctionFactor;

        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            SteeringOutput output = base.GetKinematic(agent);
            _wanderTimer += Time.deltaTime;

            if (_lastDirection == Vector3.zero)
            {
                _lastDirection = agent.transform.forward.normalized * agent.maxSpeed;
            }
            
            if (_lastMovement == Vector3.zero)
            {
                _lastMovement = agent.transform.forward;
            }

            Vector3 desiredVelocity = _lastMovement;
            if (_wanderTimer > wanderInterval)
            {
                float angle = (Random.value - Random.value) * wanderDegrees;
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * _lastDirection.normalized;
                Vector3 circleCenter = agent.transform.position + _lastMovement;
                Vector3 destination = circleCenter + direction.normalized;

                desiredVelocity = destination - agent.transform.position;
                desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;

                _lastMovement = desiredVelocity;
                _lastDirection = direction;
                _wanderTimer = 0;
            }

            if (_island != null)
            {
                Vector3 toIsland = agent.transform.position - _island.position;
                toIsland = Vector3.ProjectOnPlane(toIsland, Vector3.up);
                float currentDistance = toIsland.magnitude;
                float error = currentDistance - orbitRadius;
                
                Vector3 correctionForce = -toIsland.normalized * (error * correctionFactor);
                
                desiredVelocity += correctionForce;
                
                desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;
            }
            
            output.Linear = desiredVelocity;
            
            if (debug)
            {
                // Debug.DrawRay(transform.position, output.linear, Color.cyan);
                DebugUtils.DrawCircle(_island.position, _island.up, Color.blue, orbitRadius);
            }
            
            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            _island = agent.island.transform;
            SteeringOutput output = base.GetSteering(agent);

            output.Linear = GetKinematic(agent).Linear - agent.Velocity;
            
            return output;
        }
    }
}
