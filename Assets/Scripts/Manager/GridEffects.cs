using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using PrimeTween;

/// <summary>
/// Controla os efeitos visuais da grade de doces.
/// - Exibição animada do combo.
/// - Pulsação dos doces sugerindo possíveis movimentos.
/// - Monitoramento de inatividade do jogador para iniciar os efeitos.
/// </summary>

public class GridEffects : MonoBehaviour
{
    [Header("Combo.")]
    [Tooltip("Texto do combo na tela.")]
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("Pulse Settings.")]
    [Tooltip("Tempo de inatividade até começar a sugerir um movimento (em segundos).")]
    [SerializeField] private float idleThreshold = 5f;

    [Header("Combo Colors.")]
    [Tooltip("Cores exibidas no texto de combo, variando conforme o multiplicador.")]
    [SerializeField] private Color[] comboColors = new Color[]
    {
        new Color(1f, 0.8f, 0.9f, 1f),
        new Color(0.9f, 0.6f, 1f, 1f),
        new Color(0.6f, 0.9f, 1f, 1f),
        new Color(1f, 0.9f, 0.6f, 1f),
        new Color(0.8f, 1f, 0.6f, 1f)
    };

    #region Private Fields

    // Doces que vão pulsar
    private List<GameObject> pulsingCandies = new List<GameObject>();

    // Conta o tempo desde o último match do usuário
    private float idleTime = 0f;

    // Controle do pulsar
    private bool isPulsing = false;

    // Flag pra saber se o último match foi do usuário
    private bool userMadeMatch = false;

    /* Referências*/
    private GridManager gridManager;
    private ScoreSystem scoreSystem;

    #endregion

    private void Start()
    {
        gridManager = GridManager.Instance;
        scoreSystem = ScoreSystem.Instance;

        PrimeTweenConfig.warnEndValueEqualsCurrent = false;

        // Esconde o texto no início
        comboText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Só conta o tempo se não tá pulsando e tem movimentos possíveis
        if (!isPulsing && gridManager.HasPossibleMoves())
        {
            idleTime += Time.deltaTime;

            if (idleTime >= idleThreshold)
            {
                StartPulsing(); // Começa o pulsar depois de 5 segundos
            }
        }

        // Faz os doces pulsarem
        if (isPulsing && pulsingCandies.Count > 0)
        {
            float pulseScale = 1f + Mathf.Sin(Time.time * 5f) * 0.1f; // Pulsação suave

            foreach (var candy in pulsingCandies)
            {
                if (candy != null)
                    candy.transform.localScale = new Vector3(pulseScale, pulseScale, 1f);
            }
        }
    }

    /// <summary>
    /// Exibe o efeito visual de combo na tela, ajustando cor e escala com base no multiplicador.
    /// </summary>
    public void ShowComboEffect(int multiplier, HashSet<GameObject> matchedCandies)
    {
        if (multiplier > 1)
        {
            comboText.text = $"x{multiplier}!";
            comboText.gameObject.SetActive(true);

            // Escolhe a cor baseada no multiplier (limita ao tamanho do array)
            int colorIndex = Mathf.Min(multiplier - 2, comboColors.Length - 1);
            comboText.color = comboColors[colorIndex];

            // Aumenta o tamanho baseado no multiplier
            float scaleFactor = 1f + (multiplier - 2) * 1; // Começa em 1 e cresce 1 por combo
            comboText.transform.localScale = Vector3.zero; // Começa pequeno pra animação

            // Animação do texto do combo com PrimeTween
            Sequence comboSequence = Sequence.Create()
                .Chain(Tween.Scale(comboText.transform, new Vector3(scaleFactor, scaleFactor, 1f), 0.5f, Ease.OutBack)) // Cresce (0.5s)
                .ChainDelay(0.3f) // Pausa de 0.3s pra ficar visível antes de girar
                .Chain(Tween.Rotation(comboText.transform, new Vector3(0, 0, 360), 0.8f, Ease.InOutSine)) // Gira (0.8s)
                .Chain(Tween.Scale(comboText.transform, Vector3.zero, 0.4f, Ease.InBack)) // Encolhe (0.4s)
                .OnComplete(() => comboText.gameObject.SetActive(false)); // Esconde no final
        }
    }


    /// <summary>
    /// Reinicia o tempo de inatividade e interrompe o pulsar após um match feito pelo jogador.
    /// </summary>
    public void ResetIdleTimeAfterUserMatch()
    {
        userMadeMatch = true; // Marca que o último match foi do usuário

        idleTime = 0f; // Reseta o tempo

        StopPulsing(); // Para o pulsar imediatamente
    }


    /// <summary>
    /// Reseta o tempo de inatividade quando há matches automáticos (não feitos pelo jogador).
    /// </summary>
    public void CheckAutoMatch()
    {
        if (!userMadeMatch) // Só conta tempo se não foi o usuário que fez o match
        {
            idleTime = 0f;
        }
    }


    /// <summary>
    /// Inicia o efeito de pulsação para sugerir um movimento possível.
    /// </summary>
    public void StartPulsing()
    {
        if (pulsingCandies.Count > 0) return; // Já estava pulsando

        CandyMatchChecker matchChecker = FindObjectOfType<CandyMatchChecker>();

        if (matchChecker == null) return;

        for (int row = 0; row < gridManager.gameConfig.rows; row++)
        {
            for (int col = 0; col < gridManager.gameConfig.columns; col++)
            {
                GameObject currentCandy = gridManager.GridArray[row, col];

                if (currentCandy == null) continue;

                // Testa troca à direita
                if (col + 1 < gridManager.gameConfig.columns)
                {
                    SwapCandies(row, col, row, col + 1);
                    matchChecker.FindMatches();

                    if (matchChecker.matchedCandies.Count > 0)
                    {
                        pulsingCandies.AddRange(matchChecker.matchedCandies);
                        SwapCandies(row, col, row, col + 1); // Desfaz a troca
                        isPulsing = true;

                        return;
                    }

                    SwapCandies(row, col, row, col + 1);
                }

                // Testa troca pra baixo
                if (row + 1 < gridManager.gameConfig.rows)
                {
                    SwapCandies(row, col, row + 1, col);
                    matchChecker.FindMatches();

                    if (matchChecker.matchedCandies.Count > 0)
                    {
                        pulsingCandies.AddRange(matchChecker.matchedCandies);
                        SwapCandies(row, col, row + 1, col); // Desfaz a troca
                        isPulsing = true;

                        return;
                    }

                    SwapCandies(row, col, row + 1, col);
                }
            }
        }
    }


    /// <summary>
    /// Interrompe o efeito de pulsação e restaura a escala dos doces.
    /// </summary>
    public void StopPulsing()
    {
        if (!isPulsing) return;

        foreach (var candy in pulsingCandies)
        {
            if (candy != null)
                candy.transform.localScale = Vector3.one; // Volta ao tamanho normal
        }

        pulsingCandies.Clear();
        isPulsing = false;
    }


    /// <summary>
    /// Troca temporariamente dois doces na grade para verificar possíveis matches.
    /// </summary>
    private void SwapCandies(int row1, int col1, int row2, int col2)
    {
        GameObject temp = gridManager.GridArray[row1, col1];
        gridManager.GridArray[row1, col1] = gridManager.GridArray[row2, col2];
        gridManager.GridArray[row2, col2] = temp;
    }
}