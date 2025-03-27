using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string generalMenuScene;
    [SerializeField] private string nextLevelScene;

    [Header("Transition")]
    private SceneTransition sceneTransition;

    private void Start()
    {
        sceneTransition = FindAnyObjectByType<SceneTransition>();
    }

    public void GeneralMenu()
    {
        Time.timeScale = 1f; // Garantir que o tempo esteja normal
        sceneTransition.TransitionToScene(generalMenuScene);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f; 
        sceneTransition.TransitionToScene(nextLevelScene);
    }

    public void RepeatLeve()
    {
        Time.timeScale = 1f;
        sceneTransition.TransitionToScene(SceneManager.GetActiveScene().name);
    }

    // Sai do jogo
    public void ExitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
