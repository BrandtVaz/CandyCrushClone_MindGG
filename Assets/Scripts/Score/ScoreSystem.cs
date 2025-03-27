using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Esse script gerencia o sistema de pontua��o do jogo, acumulando pontos com base em a��es do jogador (como completar combina��es de doces) 
/// e exibindo a pontua��o em uma barra visual. Ele tamb�m calcula a pontua��o de acordo com o tamanho do "match" e o valor dos power-ups.
/// </summary>

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    private int totalScore = 0; // Pontua��o acumulada

    [Header("UI Variables.")]
    [Tooltip("Imagem da barra de progresso de pontua��o.")]
    [SerializeField] private Image scoreBarFill;
    [Tooltip("Pontua��o que o player precisa chegar.")]
    [SerializeField] private int targetScore;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreBar();

        totalScore = 0;
    }

    /// <summary>
    /// Adiciona pontos ao total, com multiplicador.
    /// </summary>
    public void AddScore(int score, int multiplier = 1)
    {
        int finalScore = score * multiplier;
        totalScore += finalScore;

        UpdateScoreBar();
    }


    /// <summary>
    /// Adiciona pontua��o com base no tamanho do match.
    /// </summary>
    public void AddMatchScore(int matchSize)
    {
        int score = CalculateMatchScore(matchSize);
        totalScore += score;

        UpdateScoreBar();
    }


    /// <summary>
    /// Adiciona pontua��o de power-up.
    /// </summary>
    public void AddPowerUpScore()
    {
        int baseMatch3Score = CalculateMatchScore(3);
        int powerUpScore = baseMatch3Score * 6;

        totalScore += powerUpScore;

        UpdateScoreBar();
    }


    /// <summary>
    /// Calcula a pontua��o com base no tamanho do match.
    /// </summary>
    public int CalculateMatchScore(int matchSize)
    {
        switch (matchSize)
        {
            case 3: return 10;
            case 4: return 20;
            case 5: return 40;
            default: return 80;
        }
    }


    /// <summary>
    /// Atualiza a barra de progresso com base na pontua��o total.
    /// </summary>
    private void UpdateScoreBar()
    {
        if (scoreBarFill != null)
        {
            float fillAmount = (float)totalScore / targetScore;
            scoreBarFill.fillAmount = Mathf.Clamp01(fillAmount);
        }
    }


    // Retorna a pontua��o total
    public int GetTotalScore()
    {
        return totalScore;
    }


    // Retorna a pontua��o alvo
    public int GetTargetScore()
    {
        return targetScore;
    }
}