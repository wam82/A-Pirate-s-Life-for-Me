using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelCollision : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherObject = other.gameObject;

        if (otherObject.CompareTag("TradeShip") || otherObject.CompareTag("PirateShip") || otherObject.CompareTag("FishingShip") || otherObject.CompareTag("Player"))
        {
            DestroyShip(otherObject);
        }
    }

    private void DestroyShip(GameObject ship)
    {
        if (ship == null)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            if (ship.CompareTag("TradeShip") || ship.CompareTag("Player"))
            {
                GameManager.Instance.RemoveTradeShip(ship);
            }
            else if (ship.CompareTag("PirateShip"))
            {
                GameManager.Instance.RemovePirate(ship);
            }
            else if (ship.CompareTag("FishingShip"))
            {
                GameManager.Instance.RemoveFishingShip(ship);
            }
        }
        
        // ship.SetActive(false);
    }
}
