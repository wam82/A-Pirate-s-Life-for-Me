using UnityEngine;

namespace Task_8.AI
{
    public abstract class AIMovement : MonoBehaviour
    {
        public bool debug;

        public virtual SteeringOutput GetKinematic(AIAgent agent)
        {
            return new SteeringOutput { Angular = agent.transform.rotation };
        }

        public virtual SteeringOutput GetSteering(AIAgent agent)
        {
            return new SteeringOutput { Angular = Quaternion.identity };
        }
    }
}