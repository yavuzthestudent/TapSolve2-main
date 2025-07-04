using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unpuzzle/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Identity")]
    public int levelNumber;

    [Header("Grid Size")]
    public int rows;       // küçük harf: LevelEditorWindow ile uyumlu
    public int columns;    // küçük harf

    [Header("Gameplay")]
    public int moveLimit;

    [Header("Cube Layout")]
    public List<CubeData> Cubes = new List<CubeData>();  // bu liste LevelEditorWindow kaydedecek
}
