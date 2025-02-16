using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishermanCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        GameObject otherObject = other.gameObject;

        if (otherObject.CompareTag("TradeShip") || otherObject.CompareTag("PirateShip") || otherObject.CompareTag("Player"))
        {
            DestroyShip(otherObject);
            DestroyShip(gameObject);
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
        }
    }
}
