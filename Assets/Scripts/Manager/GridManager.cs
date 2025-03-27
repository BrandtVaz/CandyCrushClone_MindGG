using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gerencia a criação, organização e estado do grid do jogo.
/// </summary>

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }


    public GameConfig gameConfig;


    public GameObject[,] GridArray { get; private set; }
    public GameObject[,] BackgroundTiles { get; private set; }


    private float tileSize = 1f;
    private GridUpdater gridUpdater;

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

        gridUpdater = GetComponent<GridUpdater>();
    }

    private void Start()
    {
        GenerateGrid();
        AdjustCamera();

        StartCoroutine(gridUpdater.ClearMatches());
    }

    /// <summary>
    /// Gera o grid de candies e background.
    /// </summary>
    public void GenerateGrid()
    {
        // Limpa o grid antigo
        ClearPreviousGrid();

        GridArray = new GameObject[gameConfig.rows, gameConfig.columns];
        float startX = -(gameConfig.columns - 1) / 2f * tileSize;
        float startY = (gameConfig.rows - 1) / 2f * tileSize;

        for (int row = 0; row < gameConfig.rows; row++)
        {
            for (int col = 0; col < gameConfig.columns; col++)
            {
                SpawnBackgroundTile(row, col, startX, startY);
                SpawnCandy(row, col, startX, startY);
            }
        }
    }

    private void SpawnBackgroundTile(int row, int col, float startX, float startY)
    {
        Vector2 position = new Vector2(startX + col * tileSize, startY - row * tileSize);
        Instantiate(gameConfig.tileBackgroundPrefab, position, Quaternion.identity, transform);
    }

    private void SpawnCandy(int row, int col, float startX, float startY)
    {
        GameObject candyPrefab = gameConfig.candyPrefabs[Random.Range(0, gameConfig.candyPrefabs.Length)];
        Vector2 position = new Vector2(startX + col * tileSize, startY - row * tileSize);
        GameObject newCandy = Instantiate(candyPrefab, position, Quaternion.identity, transform);
        GridArray[row, col] = newCandy;
    }


    /// <summary>
    /// Remove objetos do grid anterior, se existirem.
    /// </summary>
    private void ClearPreviousGrid()
    {
        if (GridArray != null)
        {
            foreach (var obj in GridArray)
            {
                if (obj != null)
                {
                    Destroy(obj);

                }

            }

            if (BackgroundTiles != null)
            {
                foreach (var tile in BackgroundTiles)
                {
                    if (tile != null)
                    {
                        Destroy(tile);

                    }

                }
            }
        }
    }


    /// <summary>
    /// Ajusta a câmera para enquadrar o grid.
    /// </summary>
    private void AdjustCamera()
    {
        Camera mainCamera = Camera.main;
        float gridWidth = gameConfig.columns * tileSize;
        float gridHeight = gameConfig.rows * tileSize;

        float cameraSize = Mathf.Max(gridWidth, gridHeight) / 2f + 0.5f;
        mainCamera.orthographicSize = cameraSize;
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
    }


    /// <summary>
    /// Verifica se há movimentos possíveis no grid.
    /// </summary>
    public bool HasPossibleMoves()
    {
        CandyMatchChecker matchChecker = FindObjectOfType<CandyMatchChecker>();
        if (matchChecker == null) return false;

        for (int row = 0; row < gameConfig.rows; row++)
        {
            for (int col = 0; col < gameConfig.columns; col++)
            {
                GameObject currentCandy = GridArray[row, col];
                if (currentCandy == null) continue;

                // Verificar à direita
                if (col + 1 < gameConfig.columns)
                {
                    SwapCandies(row, col, row, col + 1);

                    matchChecker.FindMatches();

                    if (matchChecker.matchedCandies.Count > 0)
                    {
                        SwapCandies(row, col, row, col + 1);
                        return true;
                    }

                    SwapCandies(row, col, row, col + 1);
                }

                // Verificar abaixo
                if (row + 1 < gameConfig.rows)
                {
                    SwapCandies(row, col, row + 1, col);

                    matchChecker.FindMatches();

                    if (matchChecker.matchedCandies.Count > 0)
                    {
                        SwapCandies(row, col, row + 1, col);

                        return true;
                    }

                    SwapCandies(row, col, row + 1, col);
                }
            }
        }
        return false;
    }


    private void SwapCandies(int row1, int col1, int row2, int col2)
    {
        GameObject temp = GridArray[row1, col1];
        GridArray[row1, col1] = GridArray[row2, col2];
        GridArray[row2, col2] = temp;
    }
}