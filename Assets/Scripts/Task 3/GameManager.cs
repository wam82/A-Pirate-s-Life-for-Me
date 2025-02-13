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

        [SerializeField] private List<GameObject> pirates = new List<GameObject>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                pirates.AddRange(GameObject.FindGameObjectsWithTag("PirateShip"));
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        public List<GameObject> GetPirates()
        {
            return new List<GameObject>(pirates);
        }
        
        public void AddPirate(GameObject pirate)
        {
            if (pirate != null && !pirates.Contains(pirate))
            {
                pirates.Add(pirate);
            }
        }
        
        public void RemovePirate(GameObject pirate)
        {
            if (pirate != null && pirates.Contains(pirate))
            {
                pirates.Remove(pirate);
            }
        }
        
        public void ClearPirates()
        {
            pirates.Clear();
        }
    }
}
