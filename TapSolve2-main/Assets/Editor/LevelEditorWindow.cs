#if UNITY_EDITOR
using System.Text;
#endif
using UnityEngine;
using UnityEditor;
using System.IO;

public class LevelEditorWindow : EditorWindow
{
    private const int DefaultCols = 4;
    private const int DefaultRows = 9;
    private const int DefaultMoveLimit = 10;

    private int _columns = DefaultCols;
    private int _rows = DefaultRows;
    private int _moveLimit = DefaultMoveLimit;

    private float _cellSize = 1f;
    private Vector2 _startPos = new Vector2(-1f, -2f);

    private Direction[,] _gridDirs;

    private LevelData _targetLevel;
    private LevelData _prevTarget;   // Seçim değişimini tespit etmek için

    [MenuItem("Window/Unpuzzle Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        EnsureGridArray();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Unpuzzle Level Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _targetLevel = (LevelData)EditorGUILayout.ObjectField(
            "Target LevelData",
            _targetLevel,
            typeof(LevelData),
            false
        );

        if (_targetLevel != _prevTarget)
        {
            LoadFromTarget();
            _prevTarget = _targetLevel;
        } 

        // 2) Grid parametreleri
        EditorGUI.BeginChangeCheck();
        _columns = EditorGUILayout.IntField("Columns", _columns);
        _rows = EditorGUILayout.IntField("Rows", _rows);
        _cellSize = EditorGUILayout.FloatField("Cell Size", _cellSize);
        _startPos = EditorGUILayout.Vector2Field("Start Pos", _startPos);
        _moveLimit = EditorGUILayout.IntField("Move Limit", _moveLimit);

        if (EditorGUI.EndChangeCheck())
            EnsureGridArray();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Directions (↑ ↓ ← →):");

        for (int y = _rows - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < _columns; x++)
            {
                _gridDirs[x, y] = (Direction)EditorGUILayout.EnumPopup(
                    _gridDirs[x, y],
                    GUILayout.Width(60)
                );
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save to LevelData"))
            SaveLevelData();

        if (GUILayout.Button("Spawn Preview"))
            SpawnPreview();
        EditorGUILayout.EndHorizontal();
    }
    private void LoadFromTarget()
    {
        if (_targetLevel == null)
        {
            _columns = DefaultCols;
            _rows = DefaultRows;
            _moveLimit = DefaultMoveLimit;
            EnsureGridArray();
            return;
        }

        _columns = _targetLevel.columns;
        _rows = _targetLevel.rows;
        _moveLimit = _targetLevel.moveLimit;
        EnsureGridArray();

        // Kaydedilmiş her CubeData için gridDirs'a aktar
        foreach (var cd in _targetLevel.Cubes)
        {
            if (cd.GridPosition.x >= 0 && cd.GridPosition.x < _columns &&
                cd.GridPosition.y >= 0 && cd.GridPosition.y < _rows)
            {
                _gridDirs[cd.GridPosition.x, cd.GridPosition.y] = cd.Direction;
            }
        }
    }

    private void EnsureGridArray()
    {
        _gridDirs = new Direction[_columns, _rows];
        for(int x= 0; x < _columns; x++)
        {
            for (int y = 0; y < _rows; y++)
            {
                _gridDirs[x, y] = Direction.Empty; // Başlangıçta tüm yönler boş 
            }
        }
    }

    // LevelData’ya kaydet
    private void SaveLevelData()
    {
        if (_targetLevel == null)
        {
            EditorUtility.DisplayDialog("Hata", "Once Target LevelData atayin!", "Tamam");
            return;
        }

        Undo.RecordObject(_targetLevel, "Save LevelData");

        _targetLevel.rows = _rows;
        _targetLevel.columns = _columns;
        _targetLevel.moveLimit = _moveLimit;
        _targetLevel.Cubes.Clear();

        for (int x = 0; x < _columns; x++)
            for (int y = 0; y < _rows; y++)
            {
                if (_gridDirs[x, y] == Direction.Empty)
                {
                    continue; // Eğer yön boşsa, bu hücreyi atla
                }
                var cd = new CubeData
                {
                    Direction = _gridDirs[x, y],
                    //Color = GetColorForDirection(_gridDirs[x, y]),
                    GridPosition = new Vector2Int(x, y)
                };
                _targetLevel.Cubes.Add(cd);
            }

        EditorUtility.SetDirty(_targetLevel);
        AssetDatabase.SaveAssets();
        Debug.Log("LevelData kaydedildi.");
    }

    private void SpawnPreview()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.EnterPlaymode();
            return;
        }

           foreach (var c in FindObjectsByType<CubeController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            CubeFactory.Instance.ReleaseCube(c);

        // Sonra yeni preview
        for (int x = 0; x < _columns; x++)
            for (int y = 0; y < _rows; y++)
            {
                Vector2Int gp = new Vector2Int(x, y);
                Vector3 wp = new Vector3(
                    _startPos.x + (x - (_columns - 1)) * _cellSize,
                    _startPos.y + y * _cellSize,
                    0f
                );
                var data = new CubeData
                {
                    Direction = _gridDirs[x, y],
                };
                Level currenLevel = FindFirstObjectByType<Level>();
                CubeFactory.Instance.SpawnCube(data, wp, currenLevel);
            }
    }
}
