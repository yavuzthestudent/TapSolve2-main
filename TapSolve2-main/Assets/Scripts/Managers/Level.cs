using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for LINQ queries

public class Level : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float _cellSize = 1f;

    [Header("Level Settings")]
    public LevelData _currentLevelData;
    private int _remainingMoves;

    // We now store the full data for each remaining cube, not just their positions.
    // This list is our "single source of truth" for the level's state.
    private List<CubeData> _activeCubesData = new List<CubeData>();
    private const string PrefsKeySavedLevelState = "SavedLevelState";

    // Singleton pattern kaldýrýldý - LevelManager zaten instance'larý yönetiyor

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

        // Try to load a saved game state.
        LoadGameState();

        bool hasProgress = _activeCubesData.Count > 0 &&
                           _activeCubesData.Count < levelData.Cubes.Count;

        // If no saved state was found, initialize from the level data.
        if (_activeCubesData.Count == 0)
        {
            // Deep copy to avoid modifying the original LevelData ScriptableObject
            foreach (var cube in levelData.Cubes)
            {
                _activeCubesData.Add(new CubeData
                {
                    GridPosition = cube.GridPosition,
                    Direction = cube.Direction,
                    LastPosition = new Vector2Int(-1, -1) // Ensure it starts as "not set"
                });
            }
            SaveGameState(); // Save the initial state
        }

        _remainingMoves = levelData.moveLimit;

        if (hasProgress)
        {
            EventManager.RaiseExistingProgress();
        }
        EventManager.RaiseMoveChanged(_remainingMoves);

        SpawnCubesFromState(); // Spawn cubes based on the loaded state
    }

    private void LoadGameState()
    {
        _activeCubesData.Clear();
        if (!PlayerPrefs.HasKey(PrefsKeySavedLevelState)) return;

        string json = PlayerPrefs.GetString(PrefsKeySavedLevelState);
        if (string.IsNullOrEmpty(json)) return;

        // Deserialize the list of cube data from JSON
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
        PlayerPrefs.Save();
    }

    // This is the core of your request!
    private void SpawnCubesFromState()
    {
        float offsetX = -((_currentLevelData.columns - 1) * 0.5f * _cellSize);
        float offsetY = -((_currentLevelData.rows - 1) * 0.5f * _cellSize);

        foreach (var cubeData in _activeCubesData)
        {
            Vector2Int spawnGridPos = cubeData.LastPosition.x != -1
                ? cubeData.LastPosition
                : cubeData.GridPosition;

            Vector3 worldPos = new Vector3(
                offsetX + spawnGridPos.x * _cellSize,
                offsetY + spawnGridPos.y * _cellSize,
                14f
            );

            var cube = CubeFactory.Instance.SpawnCube(cubeData, worldPos,this);
            // cube.Initialize(data, level) ile data referansýný ve level referansýný ata
            cube.Initialize(cubeData, this);
        }

    }

    private void HandleCubeCleared(CubeController cube)
    {
        // Find and remove the cube from our active list using its unique GridPosition
        CubeData dataToRemove = _activeCubesData.FirstOrDefault(d => d.GridPosition == cube.GetCubeData().GridPosition);
        if (dataToRemove != null)
        {
            _activeCubesData.Remove(dataToRemove);
            SaveGameState(); // Save the new state after a cube is cleared
        }

        if (_activeCubesData.Count <= 0)
        {
            EventManager.RaiseLevelComplete();
            ClearSavedCubes(); // Level complete, clear the save
        }
    }

    public void UseMove()
    {
        _remainingMoves--;
        EventManager.RaiseMoveChanged(_remainingMoves);

        // It's good practice to save progress after every move.
        SaveGameState();

        if (_remainingMoves <= 0)
        {
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

    private class CubeDataListWrapper
    {
        public List<CubeData> Cubes;
    }
}