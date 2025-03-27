using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Respons�vel por animar a troca de doces, calcular o tamanho do match, 
/// e executar efeitos visuais como part�culas.
/// </summary>
public class CandyAnimation : MonoBehaviour
{
    private GridManager gridManager;
    private GameConfig gameConfig;
    private CandyMatchChecker matchChecker;
    private ScoreSystem scoreSystem;
    private GridEffects gridEffects;

    [SerializeField] private ParticleSystem candyBurstPrefab;

    private void Start()
    {
        gridManager = GridManager.Instance;
        gameConfig = GameManager.Instance.GameConfig;
        matchChecker = FindObjectOfType<CandyMatchChecker>();
        scoreSystem = ScoreSystem.Instance;
        gridEffects = FindObjectOfType<GridEffects>();
    }

    /// <summary>
    /// Executa a anima��o de troca entre dois doces, verifica matches e atualiza o grid caso um match seja encontrado.
    /// </summary>
    public IEnumerator SwapCandies(GameObject targetCandy, Vector2Int currentPos, Vector2Int targetPos, CandyDrag candyDrag)
    {
        // Salva a posi��o inicial do doce atual (this) e o doce de destino (targetCandy)
        Vector3 startPos = transform.position;
        Vector3 targetStartPos = targetCandy.transform.position;
        float elapsedTime = 0f; // Vari�vel para o controle do tempo de anima��o

        // Enquanto o tempo de anima��o n�o acabar, move os doces at� as posi��es trocadas
        while (elapsedTime < 1f / gameConfig.candySwapSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * gameConfig.candySwapSpeed;

            // Move o doce atual e o doce de destino de forma suave
            transform.position = Vector3.Lerp(startPos, targetStartPos, t);
            targetCandy.transform.position = Vector3.Lerp(targetStartPos, startPos, t);

            yield return null;
        }

        // Ap�s a anima��o, coloca os doces nas posi��es finais
        transform.position = targetStartPos;
        targetCandy.transform.position = startPos;

        // Atualiza o GridManager para refletir a troca das posi��es no grid
        gridManager.GridArray[currentPos.y, currentPos.x] = targetCandy;
        gridManager.GridArray[targetPos.y, targetPos.x] = gameObject;

        // Verifica se h� combina��es de doces ap�s a troca
        matchChecker.FindMatches(gameObject);

        // Se houver combina��es de doces
        if (matchChecker.matchedCandies.Count > 0)
        {
            int maxMatchSize = 0;

            // Encontra o tamanho da maior combina��o de doces
            foreach (var candy in matchChecker.matchedCandies)
            {
                int matchSize = GetMatchSize(candy); // Calcula o tamanho do match para cada doce

                // Atualiza o tamanho m�ximo do match
                if (matchSize > maxMatchSize)
                    maxMatchSize = matchSize;
            }

            scoreSystem.AddMatchScore(maxMatchSize);

            HashSet<GameObject> candiesToDestroy = new HashSet<GameObject>();

            // Adiciona os doces que precisam ser destru�dos (exceto PowerUps) para a lista
            foreach (var candy in matchChecker.matchedCandies)
            {
                if (candy != null && candy.tag != "PowerUp")
                {
                    candiesToDestroy.Add(candy);
                }
            }

            // Remove os doces do grid
            foreach (var candy in candiesToDestroy)
            {
                if (candy != null)
                {
                    for (int row = 0; row < gameConfig.rows; row++)
                        for (int col = 0; col < gameConfig.columns; col++)
                            if (gridManager.GridArray[row, col] == candy)
                                gridManager.GridArray[row, col] = null;
                }
            }

            // Reseta o tempo do pulsar
            gridEffects.ResetIdleTimeAfterUserMatch();

            // Atualiza o grid ap�s a destrui��o dos doces
            yield return StartCoroutine(gridManager.GetComponent<GridUpdater>().UpdateGridAfterMatch(candiesToDestroy, candyDrag));
        }
    }


    /// <summary>
    /// Instancia e executa a part�cula de explos�o em uma posi��o espec�fica.
    /// </summary>
    public void PlayBurstParticle(Vector3 position)
    {
        if (candyBurstPrefab != null)
        {
            // Instancia o candyBurstPrefab na posi��o fornecida
            ParticleSystem burst = Instantiate(candyBurstPrefab, position, Quaternion.identity);
            burst.gameObject.SetActive(true); // Ativa
            burst.transform.SetParent(null); // Desvincula pra n�o ser afetada por outros objetos
            var main = burst.main;
            burst.Play(); // Toca a part�cula

            // Destroi a part�cula depois que terminar
            Destroy(burst.gameObject, main.duration + main.startLifetime.constant);
        }
    }


    /// <summary>
    /// Calcula o tamanho do match baseado em um doce espec�fico.
    /// </summary>
    private int GetMatchSize(GameObject candy)
    {
        int rowCount = 0; // N�mero de doces combinados na horizontal
        int colCount = 0; // N�mero de doces combinados na vertical

        // Percorre todas as linhas e colunas do grid para encontrar o doce
        for (int row = 0; row < gameConfig.rows; row++)
            for (int col = 0; col < gameConfig.columns; col++)

                // Verifica se o doce na posi��o atual � o doce que estamos analisando
                if (gridManager.GridArray[row, col] == candy)
                {
                    int tempCount = 1; // Conta o n�mero de doces na horizontal que s�o iguais ao atual
                    // Conta doces � direita
                    for (int c = col + 1; c < gameConfig.columns && gridManager.GridArray[row, c] != null && gridManager.GridArray[row, c].tag == candy.tag; c++)
                        tempCount++;

                    // Conta doces � esquerda
                    for (int c = col - 1; c >= 0 && gridManager.GridArray[row, c] != null && gridManager.GridArray[row, c].tag == candy.tag; c--)
                        tempCount++;

                    // Atualiza o maior n�mero de doces na linha
                    if (tempCount > rowCount) rowCount = tempCount;


                    tempCount = 1; // Conta o n�mero de doces na vertical que s�o iguais ao atual
                    // Conta doces abaixo
                    for (int r = row + 1; r < gameConfig.rows && gridManager.GridArray[r, col] != null && gridManager.GridArray[r, col].tag == candy.tag; r++)
                        tempCount++;

                    // Conta doces acima
                    for (int r = row - 1; r >= 0 && gridManager.GridArray[r, col] != null && gridManager.GridArray[r, col].tag == candy.tag; r--)
                        tempCount++;

                    // Atualiza o maior n�mero de doces na coluna
                    if (tempCount > colCount) colCount = tempCount;
                }

        // Retorna o maior valor entre a linha e a coluna (o tamanho do maior match)
        return Mathf.Max(rowCount, colCount);
    }
}