using System;
using UnityEngine;

public class Team : ScriptableObject
{
    [NonSerialized] public string TeamName;
    [NonSerialized] public GameObject TradeShip;
    [NonSerialized] public GameObject Harbor;

    public Team(GameObject port, GameObject ship)
    {
        TradeShip = ship;
        Harbor = port;
    }
}
