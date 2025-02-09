using UnityEngine;
using UnityEngine.Serialization;

namespace AI
{
    public class Avoidance : AIMovement
    {
        public float avoidanceRadius;
        public float avoidanceFactor;
        public override SteeringOutput GetSteering(AIAgent agent)
        {
            if (debug)
            {
                DebugUtils.DrawCircle(agent.transform.position, agent.transform.up, Color.magenta, avoidanceRadius);
            }
            SteeringOutput output = base.GetSteering(agent);
            Vector3 avoidanceForce = Vector3.zero;
            float squaredRadius = avoidanceRadius * avoidanceRadius;

            foreach (GameObject obstacle in agent.Obstacles)
            {
                Vector3 directionAway = agent.transform.position - obstacle.transform.position;
                float distanceSquared = directionAway.sqrMagnitude;

                if (distanceSquared <= squaredRadius && distanceSquared > 0)
                {
                    avoidanceForce += directionAway.normalized / distanceSquared;
                }
            }

            if (agent.Obstacles.Count > 0)
            {
                avoidanceForce = avoidanceForce.normalized * avoidanceFactor;
            }
            
            output.Linear = avoidanceForce;
            
            return output;
        }
    }
}
