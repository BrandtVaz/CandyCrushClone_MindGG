using UnityEngine;
using System.Collections.Generic;
using PrimeTween;

/// <summary>
/// Script responsável pela mecânica de arrastar e trocar doces no grid.
/// Detecta a direção do swipe, tenta realizar a troca de doces e verifica se essa troca gera um novo match.
/// Também lida com animações, como tremores quando uma troca não é válida.
/// </summary>
public class CandyDrag : MonoBehaviour
{
    private Vector2 startPos;
    [SerializeField] private float swipeThreshold = 0.5f;

    private GameConfig gameConfig;
    private GridManager gridManager;
    private CandyMatchChecker matchChecker;
    private CandyAnimation candyAnimation;
    private ScoreSystem scoreSystem;

    private void Start()
    {
        gridManager = GridManager.Instance;
        gameConfig = GameManager.Instance.GameConfig;
        matchChecker = FindObjectOfType<CandyMatchChecker>();
        scoreSystem = ScoreSystem.Instance;
        candyAnimation = GetComponent<CandyAnimation>();

        if (matchChecker == null)
        {
            Debug.LogError("CandyMatchChecker não encontrado na cena!");
        }
    }

    private void OnMouseDown()
    {
        // Registra a posição inicial do toque ou clique
        startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        // Calcula a posição final e a direção do swipe
        Vector2 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 swipeVector = endPos - startPos;

        // Verifica se o swipe atingiu o limiar de distância para ser considerado válido
        if (swipeVector.magnitude >= swipeThreshold)
        {
            // Obtém a direção do swipe
            Vector2 direction = GetSwipeDirection(swipeVector);
            TrySwap(direction);
        }
    }


    /// <summary>
    /// Define a direção do swipe, priorizando o eixo horizontal ou vertical.
    /// </summary>
    private Vector2 GetSwipeDirection(Vector2 swipeVector)
    {
        return (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
            ? (swipeVector.x > 0 ? Vector2.right : Vector2.left)
            : (swipeVector.y > 0 ? Vector2.up : Vector2.down);
    }


    /// <summary>
    /// Tenta realizar a troca entre os doces.
    /// </summary>
    private void TrySwap(Vector2 direction)
    {
        if (GameManager.IsInputLocked)
        {
            Debug.Log("Swap bloqueado: o jogo está processando algo!");

            // Realiza uma animação de piscar para indicar que o swap foi rejeitado
            Sequence blinkSequence = Sequence.Create()
                .Chain(Tween.Scale(transform, 1.2f, 0.1f, Ease.OutQuad)) // Aumenta um pouco
                .Chain(Tween.Scale(transform, 1f, 0.1f, Ease.InQuad));   // Volta ao normal
            return;
        }

        // Encontra a posição do doce no grid
        Vector2Int currentPos = FindGridPosition();
        if (currentPos == Vector2Int.one * -1) return;

        // Calcula a posição do doce de destino para a troca
        Vector2Int targetPos = currentPos + new Vector2Int((int)direction.x, -(int)direction.y);

        // Verifica se a posição de destino é válida
        if (IsValidPosition(targetPos))
        {
            GameObject targetCandy = gridManager.GridArray[targetPos.y, targetPos.x];
            bool willMatch = TestSwapForMatch(currentPos, targetPos, targetCandy);

            // Se a troca gerar um match, realiza a animação da troca
            if (willMatch)
            {
                // Gasta um movimento
                GameManager.Instance.UseMove();

                // Inicia a animação de troca dos doces
                StartCoroutine(candyAnimation.SwapCandies(targetCandy, currentPos, targetPos, this));
            }

            else
            {
                Debug.Log("Troca rejeitada: não gerou match novo.");
                ShakeCandy();
            }
        }
    }


    /// <summary>
    /// Realiza o efeito de tremor no doce quando a troca não é válida.
    /// </summary>
    private void ShakeCandy()
    {
        Transform candyTransform = transform;
        Vector3 originalPos = candyTransform.position;

        // Cria uma sequência de tremidinhas
        Sequence shakeSequence = Sequence.Create();
        float shakeDuration = 0.05f; // Duração de cada oscilação
        float shakeStrength = 0.1f; // Intensidade do tremor

        // 3 oscilações pra um efeito rápido
        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-shakeStrength, shakeStrength),
                Random.Range(-shakeStrength, shakeStrength),
                0
            );

            // Volta à posição original após o tremor
            shakeSequence.Chain(Tween.Position(candyTransform, originalPos + offset, shakeDuration, Ease.Linear));
        }

        // Volta à posição original
        shakeSequence.Chain(Tween.Position(candyTransform, originalPos, shakeDuration, Ease.Linear));
    }


    /// <summary>
    /// Testa se a troca de doces gera um novo match.
    /// </summary>
    private bool TestSwapForMatch(Vector2Int currentPos, Vector2Int targetPos, GameObject targetCandy)
    {
        // Realiza a verificação de matches antes e depois da troca
        matchChecker.FindMatches();
        HashSet<GameObject> existingMatches = new HashSet<GameObject>(matchChecker.matchedCandies);

        // Faz uma cópia do grid para simular a troca
        GameObject[,] gridCopy = (GameObject[,])gridManager.GridArray.Clone();
        gridManager.GridArray[currentPos.y, currentPos.x] = targetCandy;
        gridManager.GridArray[targetPos.y, targetPos.x] = gameObject;

        matchChecker.FindMatches();
        HashSet<GameObject> newMatches = new HashSet<GameObject>(matchChecker.matchedCandies);

        // Restaura o grid original
        gridManager.GridArray[currentPos.y, currentPos.x] = gridCopy[currentPos.y, currentPos.x];
        gridManager.GridArray[targetPos.y, targetPos.x] = gridCopy[targetPos.y, targetPos.x];

        // Retorna se a troca gerou novos matches
        newMatches.ExceptWith(existingMatches);

        return newMatches.Count > 0;
    }


    /// <summary>
    /// Encontra a posição do doce no grid.
    /// </summary>
    private Vector2Int FindGridPosition()
    {
        for (int row = 0; row < gridManager.gameConfig.rows; row++)
            for (int col = 0; col < gridManager.gameConfig.columns; col++)
                if (gridManager.GridArray[row, col] == gameObject)
                    return new Vector2Int(col, row);
        return Vector2Int.one * -1;
    }


    /// <summary>
    /// Verifica se a posição no grid é válida.
    /// </summary>
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridManager.gameConfig.columns &&
               pos.y >= 0 && pos.y < gridManager.gameConfig.rows;
    }


    // Método para destruir doces que foram combinados
    public void DestroyMatchedCandies(HashSet<GameObject> candiesToDestroy)
    {
        foreach (var candy in candiesToDestroy)
            if (candy != null)
                Destroy(candy);
    }


    // Método para tornar doces invisíveis após uma combinação
    public void InvisibleMatchedCandies(HashSet<GameObject> invisibleCandies)
    {
        foreach (var candy in invisibleCandies)
        {
            if (candy != null)
            {
                SpriteRenderer spriteRenderer = candy.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                    spriteRenderer.enabled = false;
            }
        }
    }
}