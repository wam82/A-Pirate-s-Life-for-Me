using System;
using System.Collections.Generic;
using UnityEngine;

namespace Task_3
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    _instance = obj.AddComponent<GameManager>();
                    DontDestroyOnLoad(obj);
                }

                return _instance;
            }
        }

        [SerializeField] private List<GameObject> pirateShips = new List<GameObject>();
        [SerializeField] private List<GameObject> tradeShips = new List<GameObject>();
        [SerializeField] private List<GameObject> harbors = new List<GameObject>();

        private float gameTimer = 0f;
        private bool gameOver = false;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                pirateShips.AddRange(GameObject.FindGameObjectsWithTag("PirateShip"));
                tradeShips.AddRange(GameObject.FindGameObjectsWithTag("TradeShip"));
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (!gameOver)
            {
                gameTimer += Time.deltaTime;
                CheckGameOver();
            }
        }

        public List<GameObject> GetPirates()
        {
            return new List<GameObject>(pirateShips);
        }
        
        public void AddPirate(GameObject pirate)
        {
            if (pirate != null && !pirateShips.Contains(pirate))
            {
                pirateShips.Add(pirate);
            }
        }
        
        public void RemovePirate(GameObject pirate)
        {
            if (pirate != null && pirateShips.Contains(pirate))
            {
                pirateShips.Remove(pirate);
            }
        }
        
        public void ClearPirates()
        {
            pirateShips.Clear();
        }
        
        private void CheckGameOver()
        {
            if (tradeShips.Count == 0)
            {
                GameOver("Pirates Win! All trade ships are destroyed.");
                Time.timeScale = 0;
            }
            else if (gameTimer >= 300f) 
            {
                GameOver("Time's up! Game over.");
                Time.timeScale = 0;
            }
        }
        
        private void GameOver(string message)
        {
            gameOver = true;
            Debug.Log(message);
        }
    }
}
