using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class OverlayManager : MonoBehaviour
{
    private UIDocument _uiDocument;
    private Label _timer;
    private Button _exitButton;
    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _timer = _uiDocument.rootVisualElement.Q<Label>("Time");
        _exitButton = _uiDocument.rootVisualElement.Q<Button>("Exit");
        _exitButton.RegisterCallback<ClickEvent>(evt => OnExitButtonClicked());
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Task 1" || SceneManager.GetActiveScene().name == "Task 2" || SceneManager.GetActiveScene().name == "Task 3")
        {
            _timer.text = "";
        }
        else
        {
            _timer.text = "Remaining Time: " + FormatTime(GameManager.Instance.GetRemainingTime());
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60); // Get minutes
        int seconds = Mathf.FloorToInt(timeInSeconds % 60); // Get remaining seconds

        return $"{minutes} min {seconds:D2} seconds"; // Format output with leading zero for seconds
    }
    
    private void OnExitButtonClicked()
    {
        // Debug.Log("Exit button clicked! Returning to Main Menu...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu"); // Change if needed
    }
}
