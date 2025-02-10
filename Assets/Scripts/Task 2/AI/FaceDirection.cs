using UnityEngine;

namespace Task_2.AI
{
    public class FaceDirection : AIMovement
    {
        public override SteeringOutput GetKinematic(AIAgent agent)
        {
            var output = base.GetKinematic(agent);

            if (agent.Velocity == Vector3.zero)
                return output;

            if (agent.Velocity != Vector3.zero)
                output.Angular = Quaternion.LookRotation(agent.Velocity);

            return output;
        }

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            var output = base.GetSteering(agent);

           
            if (agent.lockY)
            {
                Vector3 from = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
                Vector3 to = GetKinematic(agent).Angular * Vector3.forward;
                float angleY = Vector3.SignedAngle(from, to, Vector3.up);
                output.Angular = Quaternion.AngleAxis(angleY, Vector3.up);
            }
            else
                output.Angular = Quaternion.FromToRotation(agent.transform.forward, GetKinematic(agent).Angular * Vector3.forward);

            return output;
        }
    }
}