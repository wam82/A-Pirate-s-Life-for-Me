using UnityEngine;

namespace Task_7.AI
{
    public abstract class AIMovement : MonoBehaviour
    {
        public bool debug;

        public virtual Task_7.AI.SteeringOutput GetKinematic(Task_7.AI.AIAgent agent)
        {
            return new Task_7.AI.SteeringOutput { Angular = agent.transform.rotation };
        }

        public virtual Task_7.AI.SteeringOutput GetSteering(Task_7.AI.AIAgent agent)
        {
            return new Task_7.AI.SteeringOutput { Angular = Quaternion.identity };
        }
    }
}