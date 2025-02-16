using UnityEngine;

namespace Task_7.AI
{
    public class PathFind : AIMovement
    {
        public float fishingRadius;
        public override SteeringOutput GetSteering(AIAgent agent)
        {
            DebugUtils.DrawCircle(agent.TargetPosition, Vector3.up, Color.red, fishingRadius);
            SteeringOutput output = base.GetSteering(agent);

            Vector3 desiredVelocity = agent.TargetPosition - agent.transform.position;
            float distance = desiredVelocity.magnitude;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;

            if (distance < fishingRadius && agent.TrackedTarget.CompareTag("FishingSpot"))
            {
                agent.SetState(AIAgent.ShipState.Fishing);
                desiredVelocity *= 0;
            }
            else if (distance < fishingRadius && agent.TrackedTarget.CompareTag("Port"))
            {
                agent.SetState(AIAgent.ShipState.Fished);
                desiredVelocity *= 0;
            }
            
            output.Linear = desiredVelocity - agent.Velocity;
            
            
            return output;
        }
    }
}
