using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "ScriptableObjects/GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Board")]
    [Tooltip("N�mero de linhas no tabuleiro")]
    [SerializeField, Range(6, 8)] public int rows = 8; // Linhas do grid
    [Tooltip("N�mero de colunas no tabuleiro")]
    [SerializeField, Range(6, 8)] public int columns = 8; // Colunas do grid
    [Tooltip("Prefab do fundo das c�lulas do grid")]
    public GameObject tileBackgroundPrefab; // Fundo das c�lulas

    [Header("Candies")]
    [Tooltip("Lista de prefabs dos doces que podem ser gerados")]
    public GameObject[] candyPrefabs; // Todos os tipos de doces

    [Header("Gameplay Config")]
    [Tooltip("Velocidade de movimento dos doces ao trocar de posi��o")]
    public float candySwapSpeed = 5f; // Velocidade da anima��o de troca

    // Valida��o.
    private void OnValidate()
    {
        if (candyPrefabs == null || candyPrefabs.Length == 0)
            Debug.LogWarning("Nenhum prefab de doce configurado em GameConfig!");

        rows = Mathf.Clamp(rows, 6, 8); // Limita o grid a 6-8 linhas
        columns = Mathf.Clamp(columns, 6, 8); // Limita o grid a 6-8 colunas
        Debug.Log("Grid limitado para manter o design otimizado!");
    }
}