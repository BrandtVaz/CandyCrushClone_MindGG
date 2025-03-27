using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Esse script gerencia o sistema de pontuação do jogo, acumulando pontos com base em ações do jogador (como completar combinações de doces) 
/// e exibindo a pontuação em uma barra visual. Ele também calcula a pontuação de acordo com o tamanho do "match" e o valor dos power-ups.
/// </summary>

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    private int totalScore = 0; // Pontuação acumulada

    [Header("UI Variables.")]
    [Tooltip("Imagem da barra de progresso de pontuação.")]
    [SerializeField] private Image scoreBarFill;
    [Tooltip("Pontuação que o player precisa chegar.")]
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
    /// Adiciona pontuação com base no tamanho do match.
    /// </summary>
    public void AddMatchScore(int matchSize)
    {
        int score = CalculateMatchScore(matchSize);
        totalScore += score;

        UpdateScoreBar();
    }


    /// <summary>
    /// Adiciona pontuação de power-up.
    /// </summary>
    public void AddPowerUpScore()
    {
        int baseMatch3Score = CalculateMatchScore(3);
        int powerUpScore = baseMatch3Score * 6;

        totalScore += powerUpScore;

        UpdateScoreBar();
    }


    /// <summary>
    /// Calcula a pontuação com base no tamanho do match.
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
    /// Atualiza a barra de progresso com base na pontuação total.
    /// </summary>
    private void UpdateScoreBar()
    {
        if (scoreBarFill != null)
        {
            float fillAmount = (float)totalScore / targetScore;
            scoreBarFill.fillAmount = Mathf.Clamp01(fillAmount);
        }
    }


    // Retorna a pontuação total
    public int GetTotalScore()
    {
        return totalScore;
    }


    // Retorna a pontuação alvo
    public int GetTargetScore()
    {
        return targetScore;
    }
}