using System.Collections.Generic;
using UnityEngine;

namespace Task_4.AI
{
    public class FOVTrigger : MonoBehaviour
    {
        public AIAgent agent;
        private List<Transform> _tradeShipsInside = new List<Transform>();
        
        public Transform GetClosestTradeShip(Transform reference)
        {
            Transform closest = null;
            float minDistance = float.MaxValue;
            foreach (Transform ship in _tradeShipsInside)
            {
                float dist = Vector3.Distance(reference.position, ship.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = ship;
                }
            }
            return closest;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("TradeShip"))
            {
                agent.SetState(AIAgent.ShipState.Pursuing);
                // Debug.Log("Trade ship entered FOV: " + other.name);
                _tradeShipsInside.Add(other.transform);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("TradeShip"))
            {
                // Debug.Log("Trade ship exited FOV: " + other.name);
                _tradeShipsInside.Remove(other.transform);
            }

            if (_tradeShipsInside.Count == 0)
            {
                agent.SetState(AIAgent.ShipState.TargetLost);
            }
        }

        // Optional: For debugging or further processing, you can expose the list.
        public List<Transform> GetTradeShipsInside() => _tradeShipsInside;
    }
}