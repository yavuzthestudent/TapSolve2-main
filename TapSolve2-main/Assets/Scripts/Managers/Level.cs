using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ sorgular� i�in gerekli

public class Level : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float _cellSize = 1f;

    [Header("Level Settings")]
    public LevelData _currentLevelData;
    private int _remainingMoves;

    // Aktif k�plerin t�m bilgilerini sakl�yoruz, sadece pozisyon de�il
    // Bu liste level durumunun tek kayna�� olacak
    private List<CubeData> _activeCubesData = new List<CubeData>();
    private const string PrefsKeySavedLevelState = "SavedLevelState";

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

        // Kaydedilmi� oyun durumunu y�klemeye �al��
        LoadGameState();

        // E�er k�p say�s� 0'dan fazla ama toplam k�p say�s�ndan azsa ilerleme var ilerde y�kleme i�in kullan
        bool hasProgress = _activeCubesData.Count > 0 &&
                           _activeCubesData.Count < levelData.Cubes.Count;

        // E�er kaydedilmi� durum bulunamazsa, level verisinden ba�lat
        if (_activeCubesData.Count == 0)
        {
            foreach (var cube in levelData.Cubes)
            {
                _activeCubesData.Add(new CubeData
                {
                    GridPosition = cube.GridPosition,
                    Direction = cube.Direction,
                    LastPosition = new Vector2Int(-1, -1) // Ba�lang��ta ayarlanmam�� olarak i�aretle
                });
            }
            SaveGameState(); // �lk durumu kaydet
        }

        _remainingMoves = levelData.moveLimit;

        if (hasProgress)
        {
            EventManager.RaiseExistingProgress();
        }
        EventManager.RaiseMoveChanged(_remainingMoves);

        SpawnCubesFromState(); // Y�klenen duruma g�re k�pleri spawn et
    }

    private void LoadGameState()
    {
        _activeCubesData.Clear();
        if (!PlayerPrefs.HasKey(PrefsKeySavedLevelState)) return;

        string json = PlayerPrefs.GetString(PrefsKeySavedLevelState);
        if (string.IsNullOrEmpty(json)) return;

        // K�p verilerini JSON'dan k�p bilgisine a��l�yor.
        var savedData = JsonUtility.FromJson<CubeDataListWrapper>(json);
        if (savedData != null && savedData.Cubes != null)
        {
            _activeCubesData = savedData.Cubes;
        }
    }

    public void SaveGameState()
    {
        //Aktif olan k�pler CubeDataListWrapper s�n�f� sayesinde json'a sar�l�yor(?)
        var wrapper = new CubeDataListWrapper { Cubes = _activeCubesData };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PrefsKeySavedLevelState, json);
        PlayerPrefs.Save();
    }

    private void SpawnCubesFromState()
    {
        // Grid'i ortalamak i�in offset hesapla
        float offsetX = -((_currentLevelData.columns - 1) * 0.5f * _cellSize);
        float offsetY = -((_currentLevelData.rows - 1) * 0.5f * _cellSize);

        foreach (var cubeData in _activeCubesData)
        {
            // E�er LastPosition ayarlanm��sa oradan spawn et, yoksa orijinal pozisyondan
            Vector2Int spawnGridPos = cubeData.LastPosition.x != -1
                ? cubeData.LastPosition
                : cubeData.GridPosition;

            // Grid pozisyonunu d�nya koordinat�na �evir
            Vector3 worldPos = new Vector3(
                offsetX + spawnGridPos.x * _cellSize,
                offsetY + spawnGridPos.y * _cellSize,
                14f
            );

            var cube = CubeFactory.Instance.SpawnCube(cubeData, worldPos, this);
            // K�p� data referans� ve level referans� ile initialize et
            cube.Initialize(cubeData, this);
        }
    }

    private void HandleCubeCleared(CubeController cube)
    {
        // FirstOrDefault ile GridPosition'a g�re e�le�en ilk eleman� bul, g�nderilen Cube bilgisi �zerinden temizleyece�iz
        //foreach ile t�m k�pleri dola�mak yerine LINQ kullanarak daha verimli bir �ekilde bulduk
        //foreach ve if yap�s� da kullan�labilirdi
        CubeData dataToRemove = _activeCubesData.FirstOrDefault(d => d.GridPosition == cube.GetCubeData().GridPosition);
        if (dataToRemove != null)
        {
            _activeCubesData.Remove(dataToRemove);
            SaveGameState(); // K�p temizlendikten sonra yeni durumu kaydet
        }

        // T�m k�pler bittiyse level tamamland�
        if (_activeCubesData.Count <= 0)
        {
            EventManager.RaiseLevelComplete();
            ClearSavedCubes(); // Level tamamland�, kay�tlar� temizle
        }
    }

    public void UseMove()
    {
        _remainingMoves--;
        EventManager.RaiseMoveChanged(_remainingMoves);

        // Her hamleden sonra ilerlemeyi kaydetmek i�in
        SaveGameState();

        if (_remainingMoves <= 0)
        {
            _remainingMoves = 0; // Negatif de�ere d��meyi �nle, son g�ncellemede sorun ��kar�yordu
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