using UnityEngine;

public class PirateKill : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PirateShip"))
        {
            Debug.Log("Pirate ship died!");
            PursuitRegistry.Instance.RemovePursuit(other.gameObject.transform);
            GameManager.Instance.RemovePirate(other.gameObject);
            GameManager.Instance.scoreManager.AddPoints(transform.parent.gameObject, 25);
            other.gameObject.SetActive(false);
        }
    }
}