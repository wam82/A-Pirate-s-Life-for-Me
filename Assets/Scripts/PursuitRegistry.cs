using System.Collections.Generic;
using UnityEngine;

public class PursuitRegistry : MonoBehaviour
{
    private static PursuitRegistry _instance;
    public static PursuitRegistry Instance 
    { 
        get 
        { 
            if (_instance == null)
            {
                GameObject obj = new GameObject("PursuitRegistry");
                _instance = obj.AddComponent<PursuitRegistry>();
                DontDestroyOnLoad(obj);
            }
            return _instance; 
        }
    }

    private Dictionary<Transform, Transform> _pursuedShips = new Dictionary<Transform, Transform>();

    // Add a pursuit entry
    public void RegisterPursuit(Transform pirateShip, Transform tradeShip)
    {
        if (!_pursuedShips.ContainsKey(pirateShip))
        {
            _pursuedShips[pirateShip] = tradeShip;
        }
    }

    // Remove a pursuit entry
    public void RemovePursuit(Transform pirateShip)
    {
        if (_pursuedShips.ContainsKey(pirateShip))
        {
            _pursuedShips.Remove(pirateShip);
        }
    }

    // Check if a trade ship is being pursued
    public bool IsPursued(Transform tradeShip)
    {
        return _pursuedShips.ContainsValue(tradeShip);
    }

    // Return 
    public List<Transform> GetPursuingShip(Transform tradeShip)
    {
        List<Transform> pursuingShips = new List<Transform>();
        foreach (KeyValuePair<Transform, Transform> pair in _pursuedShips)
        {
            if (pair.Value == tradeShip)
            {
                pursuingShips.Add(pair.Key);
            }
        }
        return pursuingShips;
    }

    // Check if a pirate ship is pursuing a trade ship
    public bool IsPursuing(Transform pirateShip)
    {
        return _pursuedShips.ContainsKey(pirateShip);
    }

    // Get the trade ship a pirate is pursuing
    public Transform GetPursuedShip(Transform pirateShip)
    {
        return _pursuedShips.TryGetValue(pirateShip, out Transform tradeShip) ? tradeShip : null;
    }

    // Get all pirate ships currently pursuing trade ships
    public Dictionary<Transform, Transform> GetAllPursuits()
    {
        return new Dictionary<Transform, Transform>(_pursuedShips);
    }
}