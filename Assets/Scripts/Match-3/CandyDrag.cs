using UnityEngine;
using System.Collections.Generic;
using PrimeTween;

/// <summary>
/// Script respons�vel pela mec�nica de arrastar e trocar doces no grid.
/// Detecta a dire��o do swipe, tenta realizar a troca de doces e verifica se essa troca gera um novo match.
/// Tamb�m lida com anima��es, como tremores quando uma troca n�o � v�lida.
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
            Debug.LogError("CandyMatchChecker n�o encontrado na cena!");
        }
    }

    private void OnMouseDown()
    {
        // Registra a posi��o inicial do toque ou clique
        startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        // Calcula a posi��o final e a dire��o do swipe
        Vector2 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 swipeVector = endPos - startPos;

        // Verifica se o swipe atingiu o limiar de dist�ncia para ser considerado v�lido
        if (swipeVector.magnitude >= swipeThreshold)
        {
            // Obt�m a dire��o do swipe
            Vector2 direction = GetSwipeDirection(swipeVector);
            TrySwap(direction);
        }
    }


    /// <summary>
    /// Define a dire��o do swipe, priorizando o eixo horizontal ou vertical.
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
            Debug.Log("Swap bloqueado: o jogo est� processando algo!");

            // Realiza uma anima��o de piscar para indicar que o swap foi rejeitado
            Sequence blinkSequence = Sequence.Create()
                .Chain(Tween.Scale(transform, 1.2f, 0.1f, Ease.OutQuad)) // Aumenta um pouco
                .Chain(Tween.Scale(transform, 1f, 0.1f, Ease.InQuad));   // Volta ao normal
            return;
        }

        // Encontra a posi��o do doce no grid
        Vector2Int currentPos = FindGridPosition();
        if (currentPos == Vector2Int.one * -1) return;

        // Calcula a posi��o do doce de destino para a troca
        Vector2Int targetPos = currentPos + new Vector2Int((int)direction.x, -(int)direction.y);

        // Verifica se a posi��o de destino � v�lida
        if (IsValidPosition(targetPos))
        {
            GameObject targetCandy = gridManager.GridArray[targetPos.y, targetPos.x];
            bool willMatch = TestSwapForMatch(currentPos, targetPos, targetCandy);

            // Se a troca gerar um match, realiza a anima��o da troca
            if (willMatch)
            {
                // Gasta um movimento
                GameManager.Instance.UseMove();

                // Inicia a anima��o de troca dos doces
                StartCoroutine(candyAnimation.SwapCandies(targetCandy, currentPos, targetPos, this));
            }

            else
            {
                Debug.Log("Troca rejeitada: n�o gerou match novo.");
                ShakeCandy();
            }
        }
    }


    /// <summary>
    /// Realiza o efeito de tremor no doce quando a troca n�o � v�lida.
    /// </summary>
    private void ShakeCandy()
    {
        Transform candyTransform = transform;
        Vector3 originalPos = candyTransform.position;

        // Cria uma sequ�ncia de tremidinhas
        Sequence shakeSequence = Sequence.Create();
        float shakeDuration = 0.05f; // Dura��o de cada oscila��o
        float shakeStrength = 0.1f; // Intensidade do tremor

        // 3 oscila��es pra um efeito r�pido
        for (int i = 0; i < 3; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-shakeStrength, shakeStrength),
                Random.Range(-shakeStrength, shakeStrength),
                0
            );

            // Volta � posi��o original ap�s o tremor
            shakeSequence.Chain(Tween.Position(candyTransform, originalPos + offset, shakeDuration, Ease.Linear));
        }

        // Volta � posi��o original
        shakeSequence.Chain(Tween.Position(candyTransform, originalPos, shakeDuration, Ease.Linear));
    }


    /// <summary>
    /// Testa se a troca de doces gera um novo match.
    /// </summary>
    private bool TestSwapForMatch(Vector2Int currentPos, Vector2Int targetPos, GameObject targetCandy)
    {
        // Realiza a verifica��o de matches antes e depois da troca
        matchChecker.FindMatches();
        HashSet<GameObject> existingMatches = new HashSet<GameObject>(matchChecker.matchedCandies);

        // Faz uma c�pia do grid para simular a troca
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
    /// Encontra a posi��o do doce no grid.
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
    /// Verifica se a posi��o no grid � v�lida.
    /// </summary>
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridManager.gameConfig.columns &&
               pos.y >= 0 && pos.y < gridManager.gameConfig.rows;
    }


    // M�todo para destruir doces que foram combinados
    public void DestroyMatchedCandies(HashSet<GameObject> candiesToDestroy)
    {
        foreach (var candy in candiesToDestroy)
            if (candy != null)
                Destroy(candy);
    }


    // M�todo para tornar doces invis�veis ap�s uma combina��o
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