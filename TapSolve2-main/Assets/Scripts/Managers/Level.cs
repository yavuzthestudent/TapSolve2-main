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
        ClearExistingCubes(); // �nceki k�pleri temizle
        _currentLevelData = levelData; // Yeni seviye verisini ata

        // Kay�tl� k�p pozisyonlar�n� y�kle daha �nce oynanm��sa
        LoadRemainingCubes();

        // E�er kay�tl� veri varsa ve t�m k�pler silinmemi�se devam edilen oyun
        bool hasProgress = _remainingCubesPositions.Count > 0 &&
                          _remainingCubesPositions.Count < levelData.Cubes.Count;

        // Kay�tl� pozisyon yoksa, t�m k�pleri ba�tan ekle yeni oyun a�
        if (_remainingCubesPositions.Count == 0)
        {
            foreach (var cube in levelData.Cubes)
            {
                _remainingCubesPositions.Add(cube.GridPosition);
            }
            SaveRemainingCubes(); // T�m k�pleri kaydet
        }

        _remainingMoves = levelData.moveLimit; // Hamle s�n�r�n� ayarla
        _remainingCubes = _remainingCubesPositions.Count; // Kalan k�p say�s�n� g�ncelle

        // Event'ler ile UI'y� g�ncelle
        if (hasProgress) EventManager.RaiseExistingProgress(); // Kay�tl� oyun varsa aboneleri bilgilendir
        EventManager.RaiseMoveChanged(_remainingMoves); // Hamle say�s�n� g�ncelle

        SpawnRemainingCubes(); // Sadece kalan k�pleri olu�tur
    }

    private void LoadRemainingCubes()
    {
        _remainingCubesPositions.Clear(); // �nce listeyi temizle

        // PlayerPrefs'te kay�t varsa oku
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
                    if (coords.Length == 2) // Ge�erli bir pozisyon mu?
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

        // T�m kalan k�plerin pozisyonlar�n� "x,y" format�nda kaydet
        foreach (Vector2Int pos in _remainingCubesPositions)
        {
            positions.Add($"{pos.x},{pos.y}");
        }

        // �rnek kay�t: "2,3;5,1;4,2" (3 k�p�n pozisyonu)
        PlayerPrefs.SetString(PrefsKeyRemainingCubes, string.Join(";", positions));
        PlayerPrefs.Save(); // Diske yaz
    }

    private void SpawnRemainingCubes()
    {
        float offsetX = -((_currentLevelData.columns - 1) * 0.5f * _cellSize);
        float offsetY = -((_currentLevelData.rows - 1) * 0.5f * _cellSize);

        // Sadece kalan k�pleri spawn et
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
            // Level tamamland�, kayd� temizle
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