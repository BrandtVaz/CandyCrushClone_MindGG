using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "ScriptableObjects/Level Config", order = 1)]
public class LevelConfig : ScriptableObject
{
    [SerializeField] private string levelName = "Level 1";
    [SerializeField] private int rows = 8;
    [SerializeField] private int columns = 8;
    [SerializeField] GameObject[] candyPrefabs;
    [SerializeField] private int moves = 20;
    [SerializeField] private int targetScore = 1000;

    public string LevelName => levelName;
    public int Rows => rows;
    public int Columns => columns;
    public GameObject[] CandyPrefab => candyPrefabs;
    public int Moves => moves;
    public int TargetScore => targetScore;
}