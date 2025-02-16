using UnityEngine;

namespace Task_9.AI
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
                // Try to get the collider from the obstacle
                Collider obstacleCollider = obstacle.GetComponent<Collider>();
                if (obstacleCollider == null)
                {
                    // If there's no collider, fallback to using the transform position
                    Debug.LogWarning("Obstacle " + obstacle.name + " does not have a Collider component.");
                    continue;
                }
                
                // Get the closest point on the obstacle's collider to the agent's position
                Vector3 closestPoint = obstacleCollider.ClosestPoint(agent.transform.position);
                Vector3 directionAway = agent.transform.position - closestPoint;
                float distanceSquared = directionAway.sqrMagnitude;
            
                if (distanceSquared <= squaredRadius && distanceSquared > 0)
                {
                    avoidanceForce += directionAway.normalized / distanceSquared;
                }
                else if (distanceSquared > squaredRadius && agent.CurrentState == AIAgent.ShipState.Avoiding)
                {
                    if (agent.CompareTag("PirateShip"))
                    {
                        agent.SetState(AIAgent.ShipState.Wandering);
                    }
                    else if (agent.CompareTag("TradeShip"))
                    {
                        agent.SetState(AIAgent.ShipState.Navigating);
                    }
                }
            }
            
            if (agent.Obstacles.Count > 0)
            {
                avoidanceForce = avoidanceForce.normalized * avoidanceFactor;
            }
            avoidanceForce.y = 0;
            output.Linear = avoidanceForce - agent.Velocity;
            
            return output;
        }
    }
}
