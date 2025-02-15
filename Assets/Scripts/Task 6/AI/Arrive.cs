using UnityEngine;

namespace Task_6.AI
{
    public class Arrive : AIMovement
    {
        public float slowRadius;
        public float stopRadius;

        private void DrawDebug(AIAgent agent)
        {
            if (debug)
            {
                DebugUtils.DrawCircle(agent.TargetPosition, transform.up, Color.red, stopRadius);
                DebugUtils.DrawCircle(agent.TargetPosition, transform.up, Color.yellow, slowRadius);
            }
        }

        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            // DrawDebug(agent);

            var output = base.GetKinematic(agent);

            Vector3 desiredVelocity = agent.TargetPosition - agent.transform.position;
            float distance = desiredVelocity.magnitude;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;

            if (distance <= stopRadius)
            {
                agent.SetState(AIAgent.ShipState.Docking);
                desiredVelocity *= 0;
            }
            else if (distance < slowRadius)
            {
                desiredVelocity *= (distance / slowRadius);
            }
            
            
            output.Linear = desiredVelocity;
			
            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            DrawDebug(agent);

            var output = base.GetSteering(agent);

            output.Linear = GetKinematic(agent).Linear - agent.Velocity;

            return output;
        }
    }
}