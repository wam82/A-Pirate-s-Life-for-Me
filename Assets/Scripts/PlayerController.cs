using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public float moveSpeed;  
    [SerializeField] public float rotationSpeed;  
    [SerializeField] public Transform cameraPivot;

    private GameObject _startHarbor;
    private GameObject _endHarbor;
    
    public float stopRadius;  
    public float dockingTime;

    public List<GameObject> targetObjects; 
    private bool _isDocking = false;

    private float _moveInput;
    private float _turnInput;

    private void Start()
    {
        _startHarbor = GetClosestTarget();
    }

    void Update()
    {
        if (!_isDocking)
        {
            _moveInput = Input.GetAxis("Vertical");  // W/S or Up/Down Arrow
            _turnInput = Input.GetAxis("Horizontal");  // A/D or Left/Right Arrow

            transform.Translate(Vector3.forward * (_moveInput * moveSpeed * Time.deltaTime));

            transform.Rotate(Vector3.up * (_turnInput * rotationSpeed * Time.deltaTime));
            
            CheckDockingRadius();
        }
    }
    
    private void CheckDockingRadius()
    {
        foreach (GameObject target in targetObjects)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            
            if (distance <= stopRadius)
            {
                StartCoroutine(DockingSequence());
                break; // Stop checking once one docking condition is met
            }
        }
    }
    
    private IEnumerator DockingSequence()
    {
        _isDocking = true;
        Debug.Log("Docking started...");
        _endHarbor = GetClosestTarget();
        GameManager.Instance.scoreManager.AddPoints(gameObject, CalculateTradePrice(_startHarbor.transform.position, _endHarbor.transform.position));
        yield return new WaitForSeconds(dockingTime); // Wait before allowing movement again
        _startHarbor = _endHarbor;
        Debug.Log("Docking finished!");
        _isDocking = false;
    }
    
    private GameObject GetClosestTarget()
    {
        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject target in targetObjects)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget; 
    }

    public int CalculateTradePrice(Vector3 startHarbor, Vector3 endHarbor, float baseRate = 10f, float multiplier = 1.5f)
    {
        // Calculate the Euclidean distance between the two harbors
        float distance = Vector3.Distance(startHarbor, endHarbor);

        // Scale the price based on the distance
        int price = Mathf.RoundToInt(baseRate + (distance * multiplier));

        return price;
    }
}
