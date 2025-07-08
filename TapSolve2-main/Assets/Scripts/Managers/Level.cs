using System.Collections;
using UnityEngine;

public class Level : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float _cellSize = 1f;

    [Header("Level Settings")]
    private LevelData _currentLevelData;
    private int _remainingMoves;
    private int _remainingCubes;

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
        if (levelData == null)
        {
            Debug.LogError("LevelData is null!");
            return;
        }
        ClearExistingCubes();

        _currentLevelData = levelData;
        _remainingMoves = levelData.moveLimit;
        _remainingCubes = levelData.Cubes.Count;

        // Update UI with moves
        EventManager.RaiseMoveChanged(_remainingMoves);

        // Clear any leftover cubes then spawn new ones
        SpawnCubes();
    }

    private void SpawnCubes()
    {
        float offsetX = -((_currentLevelData.columns - 1) * 0.5f * _cellSize);
        float offsetY = -((_currentLevelData.rows - 1) * 0.5f * _cellSize);

        foreach (var cubeData in _currentLevelData.Cubes)
        {
            Vector3 worldPos = new Vector3(
                offsetX + cubeData.GridPosition.x * _cellSize,
                offsetY + cubeData.GridPosition.y * _cellSize,
                14f);

            CubeFactory.Instance.SpawnCube(cubeData, worldPos);
        }
    }

    private void ClearExistingCubes()
    {
        CubeFactory.Instance.ClearAllCubes();
    }

    public void UseMove()
    {
        _remainingMoves--;

        if (_remainingMoves <= 0)
        {
            EventManager.RaiseMoveChanged(0);
            EventManager.RaiseLevelFail();
        }
        else
        {
            EventManager.RaiseMoveChanged(_remainingMoves);
        }
    }

    private void HandleCubeCleared(CubeController cube)
    {
        _remainingCubes--;
        if (_remainingCubes <= 0)
        {
            EventManager.RaiseLevelComplete();
        }
    }
}