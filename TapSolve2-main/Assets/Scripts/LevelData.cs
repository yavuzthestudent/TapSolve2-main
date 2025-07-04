using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unpuzzle/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Identity")]
    public int levelNumber;

    [Header("Grid Size")]
    public int rows;       // k���k harf: LevelEditorWindow ile uyumlu
    public int columns;    // k���k harf

    [Header("Gameplay")]
    public int moveLimit;

    [Header("Cube Layout")]
    public List<CubeData> Cubes = new List<CubeData>();  // bu liste LevelEditorWindow kaydedecek
}
