using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OverlayManager : MonoBehaviour
{
    private UIDocument _uiDocument;
    private Label _timer;
    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _timer = _uiDocument.rootVisualElement.Q<Label>("Time");
    }

    // Update is called once per frame
    void Update()
    {
        _timer.text = "Remaining Time: " + FormatTime(GameManager.Instance.GetRemainingTime());
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60); // Get minutes
        int seconds = Mathf.FloorToInt(timeInSeconds % 60); // Get remaining seconds

        return $"{minutes} min {seconds:D2} seconds"; // Format output with leading zero for seconds
    }
}
