using UnityEngine;

namespace Task_7.AI
{
    public class FaceDirection : AIMovement
    {
        public override SteeringOutput GetSteering(Task_7.AI.AIAgent agent)
        {
            SteeringOutput output = base.GetSteering(agent);
            Quaternion angular = output.Angular;

            if (agent.Velocity == Vector3.zero) {
                angular = output.Angular;
            }

            if (agent.Velocity != Vector3.zero)
                angular = Quaternion.LookRotation(agent.Velocity);

            Vector3 from = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
            Vector3 to = angular * Vector3.forward;
            float angleY = Vector3.SignedAngle(from, to, Vector3.up);
            output.Angular = Quaternion.AngleAxis(angleY, Vector3.up);

            return output;
        }
    }
}