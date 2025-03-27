using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int totalScore = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(int score)
    {
        totalScore += score;
        Debug.Log($"Pontuação adicionada: {score}. Total: {totalScore}");
    }

    public int GetTotalScore()
    {
        return totalScore;
    }
}
