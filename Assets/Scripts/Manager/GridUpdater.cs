using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;

/// <summary>
/// gerencia a lógica da atualização de um grid de um jogo tipo "match-3". 
/// Ele é responsável por detectar matches, atualizar o grid após cada match e aplicar animações, efeitos e pontuação.
/// </summary>

public class GridUpdater : MonoBehaviour
{
    private GridManager gridManager;
    private GridEffects gridEffects;
    private ScoreSystem scoreSystem;

    public GameConfig gameConfig;

    private float tileSize = 1f;

    private void Start()
    {
        gridManager = GridManager.Instance;
        gridEffects = GetComponent<GridEffects>();
        scoreSystem = ScoreSystem.Instance;
    }

    /// <summary>
    /// Coroutine principal responsável por fazer todos os matches automáticos (cascatas) até não ter mais matches.
    /// </summary>
    public IEnumerator ClearMatches()
    {
        yield return new WaitForSeconds(0.5f); // Pequena pausa antes de começar

        gridEffects.ResetIdleTimeAfterUserMatch();

        CandyMatchChecker matchChecker = FindObjectOfType<CandyMatchChecker>();
        CandyDrag candyDrag = FindObjectOfType<CandyDrag>();

        if (matchChecker == null || candyDrag == null) yield break;

        GameManager.LockInput(); // Bloqueia input do jogador durante o processamento

        bool hasMatches = true;
        int matchSequence = 0; // Conta quantas cascatas aconteceram (para multiplicar o score)

        while (hasMatches)
        {
            matchChecker.FindMatches(); // Procura matches no grid

            if (matchChecker.matchedCandies.Count > 0)
            {
                HashSet<GameObject> candiesToDestroy = new HashSet<GameObject>(matchChecker.matchedCandies);

                // Calcula o maior tamanho de match para definir a pontuação
                int maxMatchSize = 0;
                foreach (var candy in candiesToDestroy)
                {
                    int matchSize = GetMatchSize(candy);
                    if (matchSize > maxMatchSize)
                        maxMatchSize = matchSize;
                }

                matchSequence++;
                int multiplier = matchSequence > 1 ? matchSequence : 1;
                int baseScore = scoreSystem.CalculateMatchScore(maxMatchSize);
                int multipliedScore = baseScore * multiplier;
                scoreSystem.AddScore(multipliedScore, multiplier); // Atualiza score

                gridEffects.ShowComboEffect(multiplier, candiesToDestroy);  // Efeito de combo

                // Remove as referências dos doces que serão destruídos
                foreach (var candy in candiesToDestroy)
                {
                    if (candy != null)
                    {
                        for (int row = 0; row < gridManager.gameConfig.rows; row++)
                            for (int col = 0; col < gridManager.gameConfig.columns; col++)
                                if (gridManager.GridArray[row, col] == candy)
                                    gridManager.GridArray[row, col] = null;
                    }
                }

                gridEffects.CheckAutoMatch();

                yield return StartCoroutine(UpdateGridAfterMatch(candiesToDestroy, candyDrag));
            }

            else
            {
                hasMatches = false;
            }
        }

        GameManager.UnlockInput(); // Desbloqueia o input quando termina
    }


    /// <summary>
    /// Atualiza o grid depois que os doces somem: move doces para baixo e preenche espaços vazios.
    /// </summary>
    public IEnumerator UpdateGridAfterMatch(HashSet<GameObject> candiesToDestroy, CandyDrag candyDrag, bool fromSwap = false)
    {
        GameManager.LockInput(); // Bloqueia input durante movimentação

        float startX = -(gridManager.gameConfig.columns - 1) / 2f * tileSize;
        float startY = (gridManager.gameConfig.rows - 1) / 2f * tileSize;
        List<(GameObject candy, Vector3 startPos, Vector3 targetPos)> movements = new List<(GameObject, Vector3, Vector3)>();

        candyDrag.InvisibleMatchedCandies(candiesToDestroy);  // Esconde os doces matched antes de destruir

        // Partículas de explosão para cada doce destruído
        foreach (var candy in candiesToDestroy)
        {
            if (candy != null)
            {
                CandyAnimation candyAnim = candy.GetComponent<CandyAnimation>();
                if (candyAnim != null)
                {
                    candyAnim.PlayBurstParticle(candy.transform.position);
                }
            }
        }

        // Move os doces existentes para baixo, preenchendo espaços vazios
        for (int col = 0; col < gridManager.gameConfig.columns; col++)
        {
            int emptySpaces = 0;
            for (int row = gridManager.gameConfig.rows - 1; row >= 0; row--)
            {
                if (gridManager.GridArray[row, col] == null)
                {
                    emptySpaces++;
                }

                else if (emptySpaces > 0)
                {
                    Vector3 startPos = new Vector3(startX + col * tileSize, startY - row * tileSize, 0);
                    Vector3 targetPos = new Vector3(startX + col * tileSize, startY - (row + emptySpaces) * tileSize, 0);
                    gridManager.GridArray[row + emptySpaces, col] = gridManager.GridArray[row, col];
                    gridManager.GridArray[row, col] = null;
                    movements.Add((gridManager.GridArray[row + emptySpaces, col], startPos, targetPos));
                }
            }
        }

        // Anima as movimentações para baixo
        if (movements.Count > 0)
        {
            List<Coroutine> activeCoroutines = new List<Coroutine>();
            foreach (var movement in movements)
            {
                if (movement.candy != null)
                    activeCoroutines.Add(StartCoroutine(MoveCandy(movement.candy, movement.startPos, movement.targetPos)));
            }

            foreach (var coroutine in activeCoroutines)
                yield return coroutine;
        }

        // Cria novos doces no topo para preencher vazios
        List<(GameObject candy, Vector3 startPos, Vector3 targetPos)> newCandies = new List<(GameObject, Vector3, Vector3)>();
        for (int col = 0; col < gridManager.gameConfig.columns; col++)
        {
            int emptyCount = 0;
            for (int row = gridManager.gameConfig.rows - 1; row >= 0; row--)
            {
                if (gridManager.GridArray[row, col] == null)
                    emptyCount++;
            }

            if (emptyCount > 0)
            {
                for (int i = 0; i < emptyCount; i++)
                {
                    int targetRow = i;
                    Vector3 spawnPos = new Vector3(startX + col * tileSize, startY + tileSize, 0);
                    Vector3 targetPos = new Vector3(startX + col * tileSize, startY - targetRow * tileSize, 0);
                    GameObject newCandy = Instantiate(gridManager.gameConfig.candyPrefabs[Random.Range(0, gridManager.gameConfig.candyPrefabs.Length)], spawnPos, Quaternion.identity, transform);
                    gridManager.GridArray[targetRow, col] = newCandy;
                    newCandies.Add((newCandy, spawnPos, targetPos));
                }
            }
        }

        // Anima os novos doces caindo
        if (newCandies.Count > 0)
        {
            List<Coroutine> activeCoroutines = new List<Coroutine>();
            foreach (var newCandy in newCandies)
            {
                if (newCandy.candy != null)
                    activeCoroutines.Add(StartCoroutine(MoveCandy(newCandy.candy, newCandy.startPos, newCandy.targetPos)));
            }

            foreach (var coroutine in activeCoroutines)
                yield return coroutine;
        }

        candyDrag.DestroyMatchedCandies(candiesToDestroy); // Destroi os doces que fizeram match

        // Caso o grid fique sem possíveis movimentos, embaralha
        while (!gridManager.HasPossibleMoves())
        {
            yield return StartCoroutine(ReshuffleGrid());
        }

        gridEffects.StopPulsing(); // Para o efeito de pulsar nas peças

        GameManager.UnlockInput(); // Libera input ao fim

        StartCoroutine(ClearMatches()); // Chama para detectar novas cascatas
    }


    /// <summary>
    /// Move um doce do ponto inicial ao destino com uma animação de quique no final.
    /// </summary>
    private IEnumerator MoveCandy(GameObject candy, Vector3 startPos, Vector3 targetPos)
    {
        if (candy == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < 1f / gridManager.gameConfig.candySwapSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * gameConfig.candySwapSpeed;
            candy.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        candy.transform.position = targetPos;

        // Efeito de quique quando o doce chega
        Sequence bounceSequence = Sequence.Create()
            .Chain(Tween.Scale(candy.transform, new Vector3(1.2f, 1.2f, 1f), 0.1f, Ease.OutQuad)) // Cresce rapidinho
            .Chain(Tween.Scale(candy.transform, new Vector3(0.9f, 0.9f, 1f), 0.1f, Ease.InQuad)) // Encolhe um pouco
            .Chain(Tween.Scale(candy.transform, Vector3.one, 0.1f, Ease.OutQuad)); // Volta ao normal
    }


    /// <summary>
    /// Conta o tamanho do match para calcular score.
    /// </summary>
    private int GetMatchSize(GameObject candy)
    {
        int rowCount = 0;
        int colCount = 0;

        for (int row = 0; row < gridManager.gameConfig.rows; row++)
            for (int col = 0; col < gridManager.gameConfig.columns; col++)
                if (gridManager.GridArray[row, col] == candy)
                {
                    int tempCount = 1;
                    for (int c = col + 1; c < gridManager.gameConfig.columns && gridManager.GridArray[row, c] != null && gridManager.GridArray[row, c].tag == candy.tag; c++)
                        tempCount++;
                    for (int c = col - 1; c >= 0 && gridManager.GridArray[row, c] != null && gridManager.GridArray[row, c].tag == candy.tag; c--)
                        tempCount++;
                    if (tempCount > rowCount) rowCount = tempCount;

                    tempCount = 1;
                    for (int r = row + 1; r < gridManager.gameConfig.rows && gridManager.GridArray[r, col] != null && gridManager.GridArray[r, col].tag == candy.tag; r++)
                        tempCount++;
                    for (int r = row - 1; r >= 0 && gridManager.GridArray[r, col] != null && gridManager.GridArray[r, col].tag == candy.tag; r--)
                        tempCount++;
                    if (tempCount > colCount) colCount = tempCount;
                }

        return Mathf.Max(rowCount, colCount);
    }


    /// <summary>
    /// Conta o tamanho do match para calcular score.
    /// </summary>
    public IEnumerator ReshuffleGrid()
    {
        List<(GameObject candy, Vector3 startPos)> allCandies = new List<(GameObject, Vector3)>();
        for (int row = 0; row < gridManager.gameConfig.rows; row++)
            for (int col = 0; col < gridManager.gameConfig.columns; col++)
                if (gridManager.GridArray[row, col] != null)
                    allCandies.Add((gridManager.GridArray[row, col], gridManager.GridArray[row, col].transform.position));

        for (int i = allCandies.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (GameObject candy, Vector3 startPos) temp = allCandies[i];
            allCandies[i] = allCandies[j];
            allCandies[j] = temp;
        }

        List<(GameObject candy, Vector3 startPos, Vector3 targetPos)> movements = new List<(GameObject, Vector3, Vector3)>();
        int index = 0;
        float startX = -(gridManager.gameConfig.columns - 1) / 2f * tileSize;
        float startY = (gridManager.gameConfig.rows - 1) / 2f * tileSize;

        for (int row = 0; row < gridManager.gameConfig.rows; row++)
        {
            for (int col = 0; col < gridManager.gameConfig.columns; col++)
            {
                if (gridManager.GridArray[row, col] != null)
                {
                    Vector3 targetPos = new Vector3(startX + col * tileSize, startY - row * tileSize, 0);
                    gridManager.GridArray[row, col] = allCandies[index].candy;
                    movements.Add((allCandies[index].candy, allCandies[index].startPos, targetPos));
                    index++;
                }
            }
        }

        List<Coroutine> activeCoroutines = new List<Coroutine>();
        foreach (var movement in movements)
        {
            if (movement.candy != null)
                activeCoroutines.Add(StartCoroutine(MoveCandy(movement.candy, movement.startPos, movement.targetPos)));
        }
        foreach (var coroutine in activeCoroutines)
            yield return coroutine;

        StartCoroutine(ClearMatches());
    }


    /// <summary>
    /// Para uso do debug.
    /// </summary>
    public void CallReshuffleGrid()
    {
        StartCoroutine(ReshuffleGrid());
    }
}