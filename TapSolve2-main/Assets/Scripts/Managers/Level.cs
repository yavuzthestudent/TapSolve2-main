using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Level : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float _cellSize = 5f;
    [SerializeField] private float _gridZ = 14f;
    [SerializeField] private bool _useResponsiveScaling = true;

    [Header("Level Settings")]
    public LevelData _currentLevelData;
    private int _remainingMoves;

    private List<CubeData> _activeCubesData = new List<CubeData>();
    private const string PrefsKeySavedLevelState = "SavedLevelState";
    private const string PrefsKeySavedRemainingMoves = "SavedRemainingMoves";

    private float _currentCellSize;
    private Vector3 _gridWorldCenter;
    private Bounds _viewportBounds;

    private void OnEnable()
    {
        EventManager.OnCubeCleared += HandleCubeCleared;
        EventManager.OnMoveRequested += UseMove;
    }

    private void OnDisable()
    {
        EventManager.OnCubeCleared -= HandleCubeCleared;
        EventManager.OnMoveRequested -= UseMove;
    }

    public void LoadLevel(LevelData levelData)
    {
        ClearExistingCubes();
        _currentLevelData = levelData;

        CalculateResponsiveLayout();
        LoadGameState();

        bool hasProgress = _activeCubesData.Count > 0 &&
                           _activeCubesData.Count < levelData.Cubes.Count;

        if (_activeCubesData.Count == 0)
        {
            foreach (var cube in levelData.Cubes)
            {
                _activeCubesData.Add(new CubeData
                {
                    GridPosition = cube.GridPosition,
                    Direction = cube.Direction,
                    LastPosition = new Vector2Int(-1, -1)
                });
            }
            _remainingMoves = levelData.moveLimit;
            SaveGameState();
        }
        else
        {
            _remainingMoves = PlayerPrefs.GetInt(PrefsKeySavedRemainingMoves, levelData.moveLimit);
        }

        if (hasProgress)
        {
            EventManager.RaiseExistingProgress();
        }
        EventManager.RaiseMoveChanged(_remainingMoves);

        SpawnCubesFromState();
    }

    private void CalculateResponsiveLayout()
    {
        if (!_useResponsiveScaling || _currentLevelData == null)
        {
            _currentCellSize = _cellSize;
            _gridWorldCenter = Vector3.zero;
            return;
        }

        CalculateViewportBounds();

        // Sadece ekrana ortalama - hiç boþluk býrakmama
        _currentCellSize = _cellSize;

        // Grid merkezini hesapla - sadece ekranýn tam ortasý
        _gridWorldCenter = new Vector3(
            _viewportBounds.center.x,
            _viewportBounds.center.y,
            _gridZ
        );

        Debug.Log($"[Level] CellSize: {_currentCellSize}, GridCenter: {_gridWorldCenter}");
    }

    private void CalculateViewportBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float distance = Mathf.Abs(cam.transform.position.z - _gridZ);

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, distance));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, distance));

        _viewportBounds = new Bounds(
            (bottomLeft + topRight) * 0.5f,
            topRight - bottomLeft
        );
    }

    private void LoadGameState()
    {
        _activeCubesData.Clear();
        if (!PlayerPrefs.HasKey(PrefsKeySavedLevelState)) return;

        string json = PlayerPrefs.GetString(PrefsKeySavedLevelState);
        if (string.IsNullOrEmpty(json)) return;

        var savedData = JsonUtility.FromJson<CubeDataListWrapper>(json);
        if (savedData != null && savedData.Cubes != null)
        {
            _activeCubesData = savedData.Cubes;
        }
    }

    public void SaveGameState()
    {
        var wrapper = new CubeDataListWrapper { Cubes = _activeCubesData };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PrefsKeySavedLevelState, json);

        PlayerPrefs.SetInt(PrefsKeySavedRemainingMoves, _remainingMoves);
        PlayerPrefs.Save();
    }

    private void SpawnCubesFromState()
    {
        foreach (var cubeData in _activeCubesData)
        {
            Vector2Int spawnGridPos = cubeData.LastPosition.x != -1
                ? cubeData.LastPosition
                : cubeData.GridPosition;

            Vector3 worldPos = GridToWorldPosition(spawnGridPos);

            var cube = CubeFactory.Instance.SpawnCube(cubeData, worldPos, this);
            cube.Initialize(cubeData, this);
        }
    }

    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        // Grid'i tamamen ortalama - hiç boþluk yok
        float offsetX = -((_currentLevelData.columns - 1) * 0.5f * _currentCellSize);
        float offsetY = -((_currentLevelData.rows - 1) * 0.5f * _currentCellSize);

        Vector3 localPos = new Vector3(
            offsetX + gridPos.x * _currentCellSize,
            offsetY + gridPos.y * _currentCellSize,
            0
        );

        return _gridWorldCenter + localPos;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (_currentLevelData != null)
        {
            CalculateResponsiveLayout();
            UpdateCubePositions();
        }
    }

    private void UpdateCubePositions()
    {
        var allCubes = FindObjectsOfType<CubeController>();
        foreach (var cube in allCubes)
        {
            var cubeData = cube.GetCubeData();
            Vector2Int currentGridPos = cubeData.LastPosition.x != -1
                ? cubeData.LastPosition
                : cubeData.GridPosition;

            Vector3 newWorldPos = GridToWorldPosition(currentGridPos);
            cube.transform.position = newWorldPos;
        }
    }

    public Vector3 GetGridWorldCenter() => _gridWorldCenter;
    public float GetCurrentCellSize() => _currentCellSize;
    public Bounds GetViewportBounds() => _viewportBounds;

    public Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
    {
        return GridToWorldPosition(gridPos);
    }

    public Vector2Int GetGridPositionFromWorld(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - _gridWorldCenter;

        float offsetX = -((_currentLevelData.columns - 1) * 0.5f * _currentCellSize);
        float offsetY = -((_currentLevelData.rows - 1) * 0.5f * _currentCellSize);

        int gridX = Mathf.RoundToInt((localPos.x - offsetX) / _currentCellSize);
        int gridY = Mathf.RoundToInt((localPos.y - offsetY) / _currentCellSize);

        return new Vector2Int(gridX, gridY);
    }

    private void HandleCubeCleared(CubeController cube)
    {
        CubeData dataToRemove = _activeCubesData.FirstOrDefault(d => d.GridPosition == cube.GetCubeData().GridPosition);
        if (dataToRemove != null)
        {
            _activeCubesData.Remove(dataToRemove);
            SaveGameState();
        }

        if (_activeCubesData.Count <= 0)
        {
            EventManager.RaiseLevelComplete();
            ClearSavedCubes();
        }
    }

    public void UseMove()
    {
        _remainingMoves--;
        EventManager.RaiseMoveChanged(_remainingMoves);
        SaveGameState();

        if (_remainingMoves <= 0)
        {
            _remainingMoves = 0;
            EventManager.RaiseLevelFail();
        }
    }

    public void ClearSavedCubes()
    {
        PlayerPrefs.DeleteKey(PrefsKeySavedLevelState);
        PlayerPrefs.Save();
        _activeCubesData.Clear();
    }

    private void ClearExistingCubes()
    {
        CubeFactory.Instance.ClearAllCubes();
    }

    private void OnDrawGizmos()
    {
        if (_currentLevelData == null) return;

        // Grid merkezi
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_gridWorldCenter, 0.2f);

        // Viewport sýnýrlarý
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_viewportBounds.center, _viewportBounds.size);

        // Grid sýnýrlarý
        Gizmos.color = Color.red;
        float gridWidth = _currentLevelData.columns * _currentCellSize;
        float gridHeight = _currentLevelData.rows * _currentCellSize;
        Vector3 gridSize = new Vector3(gridWidth, gridHeight, 0.1f);
        Gizmos.DrawWireCube(_gridWorldCenter, gridSize);

        // Grid hücreleri
        Gizmos.color = Color.green;
        for (int x = 0; x < _currentLevelData.columns; x++)
        {
            for (int y = 0; y < _currentLevelData.rows; y++)
            {
                Vector3 cellPos = GridToWorldPosition(new Vector2Int(x, y));
                Gizmos.DrawWireCube(cellPos, Vector3.one * _currentCellSize);
            }
        }
    }

    private class CubeDataListWrapper
    {
        public List<CubeData> Cubes;
    }
}