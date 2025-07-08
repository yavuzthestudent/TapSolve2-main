using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float _cellSize = 1f;

    [Header("Level Settings")]
    private LevelData _currentLevelData;
    private int _remainingMoves;
    private int _remainingCubes;

    private List<Vector2Int> _remainingCubesPositions = new List<Vector2Int>();
    private const string PrefsKeyRemainingCubes = "SavedRemainingCubes";

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
        ClearExistingCubes(); // Önceki küpleri temizle
        _currentLevelData = levelData; // Yeni seviye verisini ata

        // Kayýtlý küp pozisyonlarýný yükle daha önce oynanmýþsa
        LoadRemainingCubes();

        // Eðer kayýtlý veri varsa ve tüm küpler silinmemiþse devam edilen oyun
        bool hasProgress = _remainingCubesPositions.Count > 0 &&
                          _remainingCubesPositions.Count < levelData.Cubes.Count;

        // Kayýtlý pozisyon yoksa, tüm küpleri baþtan ekle yeni oyun aç
        if (_remainingCubesPositions.Count == 0)
        {
            foreach (var cube in levelData.Cubes)
            {
                _remainingCubesPositions.Add(cube.GridPosition);
            }
            SaveRemainingCubes(); // Tüm küpleri kaydet
        }

        _remainingMoves = levelData.moveLimit; // Hamle sýnýrýný ayarla
        _remainingCubes = _remainingCubesPositions.Count; // Kalan küp sayýsýný güncelle

        // Event'ler ile UI'yý güncelle
        if (hasProgress) EventManager.RaiseExistingProgress(); // Kayýtlý oyun varsa aboneleri bilgilendir
        EventManager.RaiseMoveChanged(_remainingMoves); // Hamle sayýsýný güncelle

        SpawnRemainingCubes(); // Sadece kalan küpleri oluþtur
    }

    private void LoadRemainingCubes()
    {
        _remainingCubesPositions.Clear(); // Önce listeyi temizle

        // PlayerPrefs'te kayýt varsa oku
        if (PlayerPrefs.HasKey(PrefsKeyRemainingCubes))
        {
            string savedPositions = PlayerPrefs.GetString(PrefsKeyRemainingCubes);
            if (!string.IsNullOrEmpty(savedPositions))
            {
                // Format: "x1,y1;x2,y2;x3,y3..."
                string[] positions = savedPositions.Split(';');

                foreach (string pos in positions)
                {
                    string[] coords = pos.Split(',');
                    if (coords.Length == 2) // Geçerli bir pozisyon mu?
                    {
                        int x = int.Parse(coords[0]);
                        int y = int.Parse(coords[1]);
                        _remainingCubesPositions.Add(new Vector2Int(x, y)); // Listeye ekle
                    }
                }
            }
        }
    }

    private void SaveRemainingCubes()
    {
        List<string> positions = new List<string>();

        // Tüm kalan küplerin pozisyonlarýný "x,y" formatýnda kaydet
        foreach (Vector2Int pos in _remainingCubesPositions)
        {
            positions.Add($"{pos.x},{pos.y}");
        }

        // Örnek kayýt: "2,3;5,1;4,2" (3 küpün pozisyonu)
        PlayerPrefs.SetString(PrefsKeyRemainingCubes, string.Join(";", positions));
        PlayerPrefs.Save(); // Diske yaz
    }

    private void SpawnRemainingCubes()
    {
        float offsetX = -((_currentLevelData.columns - 1) * 0.5f * _cellSize);
        float offsetY = -((_currentLevelData.rows - 1) * 0.5f * _cellSize);

        // Sadece kalan küpleri spawn et
        foreach (var cubeData in _currentLevelData.Cubes)
        {
            if (_remainingCubesPositions.Contains(cubeData.GridPosition))
            {
                Vector3 worldPos = new Vector3(
                    offsetX + cubeData.GridPosition.x * _cellSize,
                    offsetY + cubeData.GridPosition.y * _cellSize,
                    14f);

                CubeFactory.Instance.SpawnCube(cubeData, worldPos);
            }
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
        Vector2Int gridPos = cube.GetCubeData().GridPosition;
        _remainingCubesPositions.Remove(gridPos);
        SaveRemainingCubes();

        _remainingCubes--;
        if (_remainingCubes <= 0)
        {
            EventManager.RaiseLevelComplete();
            // Level tamamlandý, kaydý temizle
            PlayerPrefs.DeleteKey(PrefsKeyRemainingCubes);
        }
    }
    public void ClearSavedCubes()
    {
        PlayerPrefs.DeleteKey(PrefsKeyRemainingCubes);
        PlayerPrefs.Save();
        _remainingCubesPositions.Clear();
    }
}