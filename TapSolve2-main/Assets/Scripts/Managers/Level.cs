using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ sorgularý için gerekli

public class Level : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float _cellSize = 1f;

    [Header("Level Settings")]
    public LevelData _currentLevelData;
    private int _remainingMoves;

    // Aktif küplerin tüm bilgilerini saklýyoruz, sadece pozisyon deðil
    // Bu liste level durumunun tek kaynaðý olacak
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

        // Kaydedilmiþ oyun durumunu yüklemeye çalýþ
        LoadGameState();

        // Eðer küp sayýsý 0'dan fazla ama toplam küp sayýsýndan azsa ilerleme var ilerde yükleme için kullan
        bool hasProgress = _activeCubesData.Count > 0 &&
                           _activeCubesData.Count < levelData.Cubes.Count;

        // Eðer kaydedilmiþ durum bulunamazsa, level verisinden baþlat
        if (_activeCubesData.Count == 0)
        {
            foreach (var cube in levelData.Cubes)
            {
                _activeCubesData.Add(new CubeData
                {
                    GridPosition = cube.GridPosition,
                    Direction = cube.Direction,
                    LastPosition = new Vector2Int(-1, -1) // Baþlangýçta ayarlanmamýþ olarak iþaretle
                });
            }
            SaveGameState(); // Ýlk durumu kaydet
        }

        _remainingMoves = levelData.moveLimit;

        if (hasProgress)
        {
            EventManager.RaiseExistingProgress();
        }
        EventManager.RaiseMoveChanged(_remainingMoves);

        SpawnCubesFromState(); // Yüklenen duruma göre küpleri spawn et
    }

    private void LoadGameState()
    {
        _activeCubesData.Clear();
        if (!PlayerPrefs.HasKey(PrefsKeySavedLevelState)) return;

        string json = PlayerPrefs.GetString(PrefsKeySavedLevelState);
        if (string.IsNullOrEmpty(json)) return;

        // Küp verilerini JSON'dan küp bilgisine açýlýyor.
        var savedData = JsonUtility.FromJson<CubeDataListWrapper>(json);
        if (savedData != null && savedData.Cubes != null)
        {
            _activeCubesData = savedData.Cubes;
        }
    }

    public void SaveGameState()
    {
        //Aktif olan küpler CubeDataListWrapper sýnýfý sayesinde json'a sarýlýyor(?)
        var wrapper = new CubeDataListWrapper { Cubes = _activeCubesData };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PrefsKeySavedLevelState, json);
        PlayerPrefs.Save();
    }

    private void SpawnCubesFromState()
    {
        // Grid'i ortalamak için offset hesapla
        float offsetX = -((_currentLevelData.columns - 1) * 0.5f * _cellSize);
        float offsetY = -((_currentLevelData.rows - 1) * 0.5f * _cellSize);

        foreach (var cubeData in _activeCubesData)
        {
            // Eðer LastPosition ayarlanmýþsa oradan spawn et, yoksa orijinal pozisyondan
            Vector2Int spawnGridPos = cubeData.LastPosition.x != -1
                ? cubeData.LastPosition
                : cubeData.GridPosition;

            // Grid pozisyonunu dünya koordinatýna çevir
            Vector3 worldPos = new Vector3(
                offsetX + spawnGridPos.x * _cellSize,
                offsetY + spawnGridPos.y * _cellSize,
                14f
            );

            var cube = CubeFactory.Instance.SpawnCube(cubeData, worldPos, this);
            // Küpü data referansý ve level referansý ile initialize et
            cube.Initialize(cubeData, this);
        }
    }

    private void HandleCubeCleared(CubeController cube)
    {
        // FirstOrDefault ile GridPosition'a göre eþleþen ilk elemaný bul, gönderilen Cube bilgisi üzerinden temizleyeceðiz
        //foreach ile tüm küpleri dolaþmak yerine LINQ kullanarak daha verimli bir þekilde bulduk
        //foreach ve if yapýsý da kullanýlabilirdi
        CubeData dataToRemove = _activeCubesData.FirstOrDefault(d => d.GridPosition == cube.GetCubeData().GridPosition);
        if (dataToRemove != null)
        {
            _activeCubesData.Remove(dataToRemove);
            SaveGameState(); // Küp temizlendikten sonra yeni durumu kaydet
        }

        // Tüm küpler bittiyse level tamamlandý
        if (_activeCubesData.Count <= 0)
        {
            EventManager.RaiseLevelComplete();
            ClearSavedCubes(); // Level tamamlandý, kayýtlarý temizle
        }
    }

    public void UseMove()
    {
        _remainingMoves--;
        EventManager.RaiseMoveChanged(_remainingMoves);

        // Her hamleden sonra ilerlemeyi kaydetmek için
        SaveGameState();

        if (_remainingMoves <= 0)
        {
            _remainingMoves = 0; // Negatif deðere düþmeyi önle, son güncellemede sorun çýkarýyordu
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