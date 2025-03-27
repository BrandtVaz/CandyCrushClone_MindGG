using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// O script CandyMatchChecker é responsável por identificar combinações de doces no grid do jogo, verificando
/// linhas e colunas. Quando uma combinação de três ou mais doces do mesmo tipo é encontrada, esses doces são
/// marcados como "combinados". Se uma combinação for grande o suficiente (quatro ou mais), um power-up é criado.
/// O script também leva em consideração trocas de doces para identificar combinações pós-troca e lida com a criação
/// de power-ups conforme o tamanho da combinação encontrada.
/// </summary>
public class CandyMatchChecker : MonoBehaviour
{
    // Doces combinados
    [HideInInspector]
    public HashSet<GameObject> matchedCandies = new HashSet<GameObject>();
    // Matches em linhas
    private HashSet<GameObject> rowMatches = new HashSet<GameObject>();
    // Matches em colunas
    private HashSet<GameObject> columnMatches = new HashSet<GameObject>();

    private GridManager gridManager;
    private GameConfig gameConfig;
    private PowerUpHandler powerUpHandler;

    private void Start()
    {
        gridManager = GridManager.Instance;
        gameConfig = GameManager.Instance.GameConfig;
        powerUpHandler = GetComponent<PowerUpHandler>();
    }

    /// <summary>
    /// Procura por combinações no grid, verificando se algum doce foi trocado.
    /// </summary>
    public void FindMatches(GameObject swappedCandy = null)
    {
        if (gridManager?.GridArray == null) return;

        // Limpa as listas de matches antes de procurar novamente
        matchedCandies.Clear();
        rowMatches.Clear();
        columnMatches.Clear();

        // Inicia a verificação das linhas e colunas
        CheckMatches(swappedCandy);
    }


    /// <summary>
    /// Verifica as linhas e colunas do grid para possíveis combinações.
    /// </summary>
    private void CheckMatches(GameObject swappedCandy)
    {
        int rows = gameConfig.rows;
        int columns = gameConfig.columns;

        // Verifica cada linha do grid
        for (int row = 0; row < rows; row++)
            CheckLine(row, columns, true, swappedCandy);

        // Verifica cada coluna do grid
        for (int col = 0; col < columns; col++)
            CheckLine(col, rows, false, swappedCandy);
    }


    /// <summary>
    /// Verifica uma linha ou coluna por possíveis matches.
    /// </summary>
    private void CheckLine(int index, int length, bool isRow, GameObject swappedCandy)
    {
        int matchCount = 1; // Contador de doces combinados
        GameObject previousCandy = null; // Doce anterior para comparação
        List<GameObject> potentialMatch = new List<GameObject>(); // Lista de possíveis doces para combinação

        for (int i = 0; i < length; i++)
        {
            // Obtém o doce atual na linha ou coluna
            GameObject currentCandy = isRow ? gridManager.GridArray[index, i] : gridManager.GridArray[i, index];

            // Verifica se o doce atual é igual ao anterior
            if (currentCandy != null && previousCandy != null && currentCandy.tag == previousCandy.tag)
            {
                matchCount++;
                potentialMatch.Add(currentCandy);
            }

            else
            {
                if (matchCount >= 3)
                {
                    // Se houver um match, adiciona à lista
                    foreach (var candy in potentialMatch)
                    {
                        matchedCandies.Add(candy);

                        if (isRow) rowMatches.Add(candy);
                        else columnMatches.Add(candy);
                    }

                    matchedCandies.Add(previousCandy);

                    if (isRow) rowMatches.Add(previousCandy);
                    else columnMatches.Add(previousCandy);

                    // Se for um match grande, cria um power-up
                    if (matchCount >= 4 && swappedCandy != null && (potentialMatch.Contains(swappedCandy) || previousCandy == swappedCandy))
                    {
                        powerUpHandler.CreatePowerUp(swappedCandy, matchCount);
                    }
                }

                // Reseta o contador para o próximo doce
                matchCount = 1;

                potentialMatch.Clear();

                if (currentCandy != null)
                    potentialMatch.Add(currentCandy);
            }

            previousCandy = currentCandy;
        }

        // Verifica o último doce da linha/coluna
        if (matchCount >= 3)
        {
            foreach (var candy in potentialMatch)
            {
                matchedCandies.Add(candy);

                if (isRow) rowMatches.Add(candy);
                else columnMatches.Add(candy);
            }

            matchedCandies.Add(previousCandy);

            if (isRow) rowMatches.Add(previousCandy);
            else columnMatches.Add(previousCandy);

            // Cria um power-up para matches grandes
            if (matchCount >= 4 && swappedCandy != null && (potentialMatch.Contains(swappedCandy) || previousCandy == swappedCandy))
            {
                powerUpHandler.CreatePowerUp(swappedCandy, matchCount);
            }
        }
    }
}