using UnityEngine;

namespace Task_2.AI
{
    public class Flee : AIMovement
    {
        public override SteeringOutput GetSteering(AIAgent agent)
        {
            // Debug.Log("Reached");
            SteeringOutput output = base.GetSteering(agent);
            
            Vector3 desiredVelocity = agent.TargetPosition - agent.transform.position;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;
            Vector3 steering = desiredVelocity - agent.Velocity;
            output.Linear = -steering;
            
            return output;
        }
    }
}
