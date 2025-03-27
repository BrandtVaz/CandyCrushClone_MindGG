using UnityEngine;
using System.Collections.Generic;
using TMPro;
using PrimeTween;
using System.Collections;

/// <summary>
/// Esse script é responsável por criar, ativar e gerenciar o comportamento dos power-ups dentro do jogo. 
/// Ele lida com a criação de power-ups quando há um match grande (mais de 3 doces combinados) e implementa efeitos visuais, 
/// como o "pulso" do power-up antes de sua ativação. O efeito de explosão destrói os doces ao redor e aumenta a pontuação do jogador.
/// </summary>

public class PowerUpHandler : MonoBehaviour
{
    // Sprite do power-up
    [SerializeField] private Sprite powerUpSprite;
    // Tempo em segundos que o power-up pulsa antes de explodir
    [SerializeField] private float pulsing = 2f;

    private GridManager gridManager;
    private ScoreSystem scoreSystem;
    private GameConfig gameConfig;

    private void Start()
    {
        gridManager = GridManager.Instance;
        scoreSystem = ScoreSystem.Instance;
        gameConfig = GameManager.Instance.GameConfig;
    }

    /// <summary>
    /// Cria um power-up a partir de um match grande.
    /// </summary>
    public void CreatePowerUp(GameObject candy, int matchCount)
    {
        // Bloqueia o input enquanto o power-up está ativo
        GameManager.LockInput();

        candy.tag = "PowerUp";
        SetCandySprite(candy, powerUpSprite);

        StartCoroutine(ActivatePowerUp(candy));
    }


    /// <summary>
    /// Define o sprite do power-up.
    /// </summary>
    private void SetCandySprite(GameObject candy, Sprite sprite)
    {
        // Atualiza o sprite do doce para o sprite do power-up
        if (candy.TryGetComponent<SpriteRenderer>(out var renderer))
            renderer.sprite = sprite;
    }


    /// <summary>
    /// Ativa o efeito do power-up.
    /// </summary>
    private IEnumerator ActivatePowerUp(GameObject powerUp)
    {
        float pulseTime = 0f;

        // Cria o efeito de pulso no power-up
        while (pulseTime < pulsing)
        {
            pulseTime += Time.deltaTime;
            float pulseScale = 1f + Mathf.Sin(Time.time * 5f) * 0.2f; // Pulsação do poder do doce

            if (powerUp != null)
            {
                powerUp.transform.localScale = new Vector3(pulseScale, pulseScale, 1f);
            }

            yield return null;
        }

        // Após o pulso, define o tamanho do power-up para o padrão
        if (powerUp != null)
            powerUp.transform.localScale = Vector3.one;

        // Identifica a posição do power-up no grid
        Vector2Int powerUpPos = Vector2Int.one * -1;
        for (int row = 0; row < gridManager.gameConfig.rows; row++)
            for (int col = 0; col < gridManager.gameConfig.columns; col++)
                if (gridManager.GridArray[row, col] == powerUp)
                    powerUpPos = new Vector2Int(col, row);

        if (powerUpPos != Vector2Int.one * -1)
        {
            HashSet<GameObject> candiesToExplode = new HashSet<GameObject>();

            // Identifica os doces ao redor do power-up para serem destruídos
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    int newX = powerUpPos.x + dx;
                    int newY = powerUpPos.y + dy;
                    if (newX >= 0 && newX < gridManager.gameConfig.columns && newY >= 0 && newY < gridManager.gameConfig.rows)
                    {
                        GameObject candy = gridManager.GridArray[newY, newX];
                        if (candy != null && candy != powerUp)
                            candiesToExplode.Add(candy);
                    }
                }
            }

            // Remove os doces ao redor do power-up
            foreach (var candy in candiesToExplode)
            {
                for (int row = 0; row < gridManager.gameConfig.rows; row++)
                    for (int col = 0; col < gridManager.gameConfig.columns; col++)
                        if (gridManager.GridArray[row, col] == candy)
                            gridManager.GridArray[row, col] = null;
            }

            // Remove o power-up do grid
            candiesToExplode.Add(powerUp);
            for (int row = 0; row < gridManager.gameConfig.rows; row++)
                for (int col = 0; col < gridManager.gameConfig.columns; col++)
                    if (gridManager.GridArray[row, col] == powerUp)
                        gridManager.GridArray[row, col] = null;

            // Ativa os efeitos visuais do power-up
            powerUp.GetComponent<PowerUpParticle>().ActivateParticle();

            // Abala a tela para dar uma sensação de impacto
            ShakeScreen();
            // Aumenta a pontuação do jogador
            scoreSystem.AddPowerUpScore();

            // Atualiza o grid após a explosão dos doces
            CandyDrag candyDrag = FindObjectOfType<CandyDrag>();

            yield return StartCoroutine(gridManager.GetComponent<GridUpdater>().UpdateGridAfterMatch(candiesToExplode, candyDrag));
        }

        // Desbloqueia o input quando o power-up termina
        GameManager.UnlockInput();
    }


    /// <summary>
    /// Efeito de tremor na tela para dar mais impacto visual.
    /// </summary>
    private void ShakeScreen(float duration = 0.3f, float strength = 0.2f)
    {
        Transform camTransform = Camera.main.transform;
        Vector3 originalPos = camTransform.position;

        Sequence shakeSequence = Sequence.Create();

        // Faz 5 oscilações rápidas para simular o tremor
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-strength, strength),
                Random.Range(-strength, strength),
                0);

            shakeSequence.Chain(Tween.Position(camTransform, originalPos + randomOffset, duration / 10f, Ease.Linear));
        }

        // Volta à posição original da câmera
        shakeSequence.Chain(Tween.Position(camTransform, originalPos, duration / 10f, Ease.Linear)); // Volta à posição original
    }
}