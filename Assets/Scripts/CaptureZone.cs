using UnityEngine;

public class CaptureZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TradeShip"))
        {
            Debug.Log("Trade ship died!");
            other.gameObject.SetActive(false);
        }
    }
}