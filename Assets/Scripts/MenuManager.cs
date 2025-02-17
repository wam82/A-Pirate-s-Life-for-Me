using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    private UIDocument _uiDocument;
    private Button _task1;
    private Button _task2;
    private Button _task3;
    private Button _task4;
    private Button _task5;
    private Button _task6;
    private Button _task7;
    private Button _task8;
    private Button _task9;
    private Button _task10;

    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _task1 = _uiDocument.rootVisualElement.Q<Button>("Task1");
        _task2 = _uiDocument.rootVisualElement.Q<Button>("Task2");
        _task3 = _uiDocument.rootVisualElement.Q<Button>("Task3");
        _task4 = _uiDocument.rootVisualElement.Q<Button>("Task4");
        _task5 = _uiDocument.rootVisualElement.Q<Button>("Task5");
        _task6 = _uiDocument.rootVisualElement.Q<Button>("Task6");
        _task7 = _uiDocument.rootVisualElement.Q<Button>("Task7");
        _task8 = _uiDocument.rootVisualElement.Q<Button>("Task8");
        _task9 = _uiDocument.rootVisualElement.Q<Button>("Task9");
        _task10 = _uiDocument.rootVisualElement.Q<Button>("Task10");
        _task1.RegisterCallback<ClickEvent>(evt => OnTaskButtonClicked(1));
        _task2.RegisterCallback<ClickEvent>(evt => OnTaskButtonClicked(2));
        _task3.RegisterCallback<ClickEvent>(evt => OnTaskButtonClicked(3));
        _task4.RegisterCallback<ClickEvent>(evt => OnTaskButtonClicked(4));
        _task5.RegisterCallback<ClickEvent>(evt => OnTaskButtonClicked(5));
        _task6.RegisterCallback<ClickEvent>(evt => OnTaskButtonClicked(6));
        _task7.RegisterCallback<ClickEvent>(evt => OnTaskButtonClicked(7));
        _task8.RegisterCallback<ClickEvent>(evt => OnTaskButtonClicked(8));
        _task9.RegisterCallback<ClickEvent>(evt => OnTaskButtonClicked(9));
        _task10.RegisterCallback<ClickEvent>(evt => OnTaskButtonClicked(10));
    }
    private void OnTaskButtonClicked(int taskNumber)
    {
        // Debug.Log($"Task {taskNumber} button clicked!");
        LoadTaskScene(taskNumber);
    }

// Example Function to Load Task Scene
    private void LoadTaskScene(int taskNumber)
    {
        string sceneName = $"Task {taskNumber}"; 
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}