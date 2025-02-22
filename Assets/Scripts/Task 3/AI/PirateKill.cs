using UnityEngine;

namespace Task_3.AI
{
    public class PirateKill : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("PirateShip"))
            {
                Debug.Log("Pirate ship died!");
                PursuitRegistry.Instance.RemovePursuit(other.gameObject.transform);
                GameManager.Instance.RemovePirate(other.gameObject);
                other.gameObject.SetActive(false);
            }
        }
    }
}
