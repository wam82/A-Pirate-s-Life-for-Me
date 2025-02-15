using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    private Dictionary<Team, int> _teamScore = new Dictionary<Team, int>();
    
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optionally, persist across scenes:
        // DontDestroyOnLoad(gameObject);
    }

    public void AddTeam(Team team)
    {
        _teamScore.Add(team, 0);
    }

    public void AddPoints(GameObject obj, int score)
    {
        Team team = FindTeam(obj);
        if (!_teamScore.ContainsKey(team))
        {
            _teamScore[team] = 0;
        }
        else
        {
            _teamScore[team] += score;
        }
        // Debug.LogWarning(GetScore(team));
    }

    public int GetScore(Team team)
    {
        if (_teamScore.ContainsKey(team))
        {
            return _teamScore[team];
        }
        return 0;
    }

    public Dictionary<Team, int> GetTeamScores()
    {
        return new Dictionary<Team, int>(_teamScore);
    }

    private Team FindTeam(GameObject obj)
    {
        Team team = null;
        foreach (Team t in _teamScore.Keys)
        {
            if (t.Harbor == obj || t.TradeShip == obj)
            {
                team = t;
            }
        }
        return team;
    }
}
