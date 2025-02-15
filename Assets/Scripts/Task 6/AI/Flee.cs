using System.Collections.Generic;
using UnityEngine;

namespace Task_6.AI
{
    public class Flee : AIMovement
    {
        public float seekWeight;
        public float velocityMatchWeight;
        public override SteeringOutput GetSteering(AIAgent agent)
        {
            // Debug.Log("Reached");
            SteeringOutput output = base.GetSteering(agent);
            
            // 1. Compute the 'seek' harbor (tracked target) force
            Vector3 desiredDirection = (agent.TargetPosition - agent.transform.position).normalized;
            Vector3 desiredVelocity = desiredDirection * agent.maxSpeed;
            Vector3 seekForce = desiredVelocity - agent.Velocity;
            
            // 2. Compute the average velocity of the pursuing pirates
            List<Transform> pursuingPirates = agent.pursuingPirates;
            Vector3 averagePirateVelocity = Vector3.zero;
            float slowestMaxSpeed = float.MaxValue;
            int count = 0;
            foreach (Transform pirate in pursuingPirates)
            {
                AIAgent pirateAgent = pirate.GetComponent<AIAgent>();
                if (pirateAgent != null)
                {
                    if (pirateAgent.maxSpeed < slowestMaxSpeed)
                    {
                        slowestMaxSpeed = pirateAgent.maxSpeed;
                    }
                    averagePirateVelocity += pirateAgent.Velocity;
                    count++;
                }
            }
            if (count > 0)
            {
                averagePirateVelocity /= count;
            }
            else
            {
                Debug.Log("Something went wrong. No pirates where found in seek");
            }
            // seekForce = Vector3.ClampMagnitude(seekForce, slowestMaxSpeed);
            // 3. Compute the matching velocity
            Vector3 velocityMatchingForce = averagePirateVelocity - agent.Velocity;
            
            // 4. Blend all forces
            Vector3 combinedForce = (seekWeight * seekForce) + (velocityMatchWeight * velocityMatchingForce);
            // combinedForce = Vector3.ClampMagnitude(combinedForce, slowestMaxSpeed);
            output.Linear = combinedForce;
            
            return output;
        }
    }
}
