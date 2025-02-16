using UnityEngine;

namespace Task_7.AI
{
    public class Sneak : AIMovement
    {
        public float bufferDistance;
        public float slowRadius;
        public float crowdingSeparation;
        private Transform _leader;
        private float _desiredSpeed;
        private Vector3 _desiredVelocity;
        public override SteeringOutput GetSteering(Task_7.AI.AIAgent agent)
        {
            SteeringOutput output = base.GetSteering(agent);
            _leader = agent.TrackedTarget;
            _desiredSpeed = agent.maxSpeed;
            
            // 1. Find the Right Spot to Follow
            Vector3 followOffset = -_leader.forward * bufferDistance;
            Vector3 followTarget = _leader.position + followOffset;
            
            Debug.DrawLine(_leader.position, followTarget, Color.red);
            // DebugUtils.DrawCircle(_leader.position, Vector3.up, Color.green, slowRadius);
            
            // 2. Following & Arriving (Smooth Arrival)
            // Debug.DrawLine(agent.transform.position, followTarget, Color.cyan);
            Vector3 direction = followTarget - agent.transform.position;
            // Debug.DrawLine(agent.transform.position, direction, Color.blue);
            float distance = direction.magnitude;
            if (distance < slowRadius && distance > bufferDistance)
            {
                // Debug.Log("In slow distance");
                _desiredSpeed = agent.maxSpeed * (distance / slowRadius);
            }

            _desiredVelocity = direction;
            _desiredVelocity.Normalize();
            _desiredVelocity *= _desiredSpeed;
            output.Linear = _desiredVelocity;
            
            // 3. Avoid Crowding (Maintain a buffer from the pirate ship)
            // float leaderDistance = Vector3.Distance(agent.transform.position, _leader.position);
            // if (leaderDistance < crowdingSeparation)
            // {
            //     output.Linear += (agent.transform.position - _leader.position).normalized * agent.maxSpeed;
            // }
            
            // 4. If you ended up in the docking zone whilst sneaking
            Vector3 desiredVelocity = agent.latestPort.position - agent.transform.position;
            float distanceFromPort = desiredVelocity.magnitude;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;
            
            if (distanceFromPort <= agent.GetComponent<Task_7.AI.Arrive>().stopRadius)
            {
                agent.SetState(Task_7.AI.AIAgent.ShipState.Docking);
                desiredVelocity *= 0;
            }
            
            return output;
        }
    }
}