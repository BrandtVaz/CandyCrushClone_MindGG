using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gerencia o estado geral do jogo, incluindo pontuação, movimentos restantes, menus e controle de pausa.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager Instance { get; private set; }

    #endregion

    #region Config

    [Header("Game Config")]
    [SerializeField] private GameConfig gameConfig;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject winMenu;

    [Header("Game State")]
    [SerializeField] private int movesLeft;
    private int currentScore;
    private bool isPaused = false;
    public static bool IsInputLocked { get; private set; } = false;

    #endregion

    #region Properties

    public GameConfig GameConfig => gameConfig;

    #endregion

    #region Unity Methods

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
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        winMenu.SetActive(false);

        UpdateMovesUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    #endregion

    #region Game Logic

    /// <summary>
    /// Gasta um movimento e verifica condições de fim de jogo.
    /// </summary>
    public void UseMove()
    {
        movesLeft--;
        UpdateMovesUI();
        CheckGameOver();
    }

    /// <summary>
    /// Adiciona pontos ao placar e verifica condições de vitória.
    /// </summary>
    /// <param name="score">Quantidade de pontos a adicionar.</param>
    public void AddScore(int score)
    {
        ScoreSystem.Instance.AddScore(score);
        CheckGameOver();
    }

    /// <summary>
    /// Atualiza o texto de movimentos restantes na UI.
    /// </summary>
    private void UpdateMovesUI()
    {
        if (movesText != null)
            movesText.text = movesLeft.ToString();
    }

    /// <summary>
    /// Verifica se o jogo terminou ou se o jogador venceu.
    /// </summary>
    private void CheckGameOver()
    {
        if (movesLeft <= 0 && ScoreSystem.Instance.GetTotalScore() < ScoreSystem.Instance.GetTargetScore())
        {
            Debug.Log("Fim de jogo! Sem movimentos restantes.");

            Time.timeScale = 0f;
            gameOverMenu.SetActive(true);
        }

        else if (ScoreSystem.Instance.GetTotalScore() >= ScoreSystem.Instance.GetTargetScore())
        {
            Debug.Log("Nível concluído!");

            Time.timeScale = 1f;
            winMenu.SetActive(true);
        }
    }

    #endregion

    #region Pause

    /// <summary>
    /// Pausa o jogo e exibe o menu de pausa.
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        pauseMenu.SetActive(true);
    }

    /// <summary>
    /// Retoma o jogo e oculta o menu de pausa.
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseMenu.SetActive(false);
    }

    #endregion

    #region Debug & Input

    /// <summary>
    /// Adiciona movimentos extras (apenas para debug).
    /// </summary>
    public void MoreMoves()
    {
        movesLeft += 10;
        UpdateMovesUI();
    }

    /// <summary>
    /// Bloqueia o input do jogador.
    /// </summary>
    public static void LockInput() => IsInputLocked = true;

    /// <summary>
    /// Desbloqueia o input do jogador.
    /// </summary>
    public static void UnlockInput() => IsInputLocked = false;

    #endregion
}
