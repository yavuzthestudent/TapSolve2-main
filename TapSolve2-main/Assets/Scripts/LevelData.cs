using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber;
    public int moveLimit = 20;

    [Header("Grid Settings")]
    public int rows = 2;
    public int columns = 2;
}