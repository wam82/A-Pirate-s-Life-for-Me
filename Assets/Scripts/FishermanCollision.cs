using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishermanCollision : MonoBehaviour
{
    private bool _isDestroyed = false;

    private void OnCollisionEnter(Collision other)
    {
        if (_isDestroyed)
        {
            return;
        }
        
        GameObject otherObject = other.gameObject;

        if (otherObject.CompareTag("TradeShip") || otherObject.CompareTag("PirateShip"))
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
            if (ship.CompareTag("TradeShip"))
            {
                GameManager.Instance.RemoveTradeShip(ship);
            }
            else if (ship.CompareTag("PirateShip"))
            {
                GameManager.Instance.RemovePirate(ship);
            }
        }
        
        ship.SetActive(false);
        _isDestroyed = true;
    }
}
