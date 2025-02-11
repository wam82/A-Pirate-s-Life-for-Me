using UnityEngine;

namespace Task_3.AI
{
    public class Sneak : AIMovement
    {
        public float bufferDistance;
        public float arrivalThreshold;
        public float crowdingSeparation;
        private Transform _leader;
        public override SteeringOutput GetSteering(AIAgent agent)
        {
            SteeringOutput output = base.GetSteering(agent);
            _leader = agent.TrackedTarget;
            
            // 1. Find the Right Spot to Follow
            Vector3 followOffset = -_leader.forward * bufferDistance;
            Vector3 followTarget = _leader.position + followOffset;
            
            Debug.DrawLine(_leader.position, followTarget, Color.red);
            
            // 2. Following & Arriving (Smooth Arrival)
            Vector3 toTarget = followTarget - agent.transform.position;
            float followDistance = toTarget.magnitude;

            if (followDistance < arrivalThreshold)
            {
                DebugUtils.DrawCircle(_leader.position, Vector3.up, Color.black, arrivalThreshold);
                // Debug.Log("Slowing Down");
                // Slow down as we get closer
                output.Linear = toTarget.normalized * (agent.maxSpeed * (followDistance / arrivalThreshold));
            }
            else
            {
                // Debug.Log("Accelerating");
                // Move at max speed
                output.Linear = toTarget.normalized * agent.maxSpeed;
            }
            
            // 3. Avoid Crowding (Maintain a buffer from the pirate ship)
            float leaderDistance = Vector3.Distance(agent.transform.position, _leader.position);
            if (leaderDistance < crowdingSeparation)
            {
                output.Linear += (agent.transform.position - _leader.position).normalized * agent.maxSpeed;
            }
            
            // 4. If you ended up in the docking zone whilst sneaking
            Vector3 desiredVelocity = agent.TargetPosition - agent.transform.position;
            float distance = desiredVelocity.magnitude;
            if (distance <= agent.GetComponent<Arrive>().stopRadius)
            {
                agent.SetState(AIAgent.ShipState.Docking);
                output.Linear *= 0;
            }
            
            return output;
        }
    }
}