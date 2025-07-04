using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; set; }

    [Header("All Levels")]
    [SerializeField] private LevelData[] _levels;

    private int _currentLevelIndex = 0;
    private int _remainingCubes;
    private int _actualLevelNumber = 1; //Ekranda gösterilecek gerçek seviye numarasý, index için kullanýlacak deðil

    [Header("Grid Settings (Runtime)")]
    [SerializeField] private Vector2 _startPos = new Vector2(0f, 0f);
    [SerializeField] private float _cellSize = 1f;

    [SerializeField] private int _columns = 4;
    [SerializeField] private int _rows = 9;

    //private SmartCubeSpawner _cubeSpawner;
    private UIManager _uiManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        //_cubeSpawner = FindFirstObjectByType<SmartCubeSpawner>(); // Updated method  

        LoadLevel(_currentLevelIndex);
    }

    private void OnEnable()
    {
        EventManager.OnCubeCleared += HandleCubeCleared;
        EventManager.OnLevelCompleted += HandleLevelCompleted;
        EventManager.OnLevelFail += HandleLevelFailed;
        EventManager.OnMoveChanged += HandleMoveChanged;
    }

    private void OnDisable()
    {
        EventManager.OnCubeCleared -= HandleCubeCleared;
        EventManager.OnLevelCompleted -= HandleLevelCompleted;
        EventManager.OnLevelFail -= HandleLevelFailed;
        EventManager.OnMoveChanged -= HandleMoveChanged;

    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _levels.Length)
        {
            Debug.LogError("Invalid level index: " + levelIndex);
            return;
        }
        _currentLevelIndex = levelIndex;
        LevelData data = _levels[levelIndex];

        UIManager.Instance.UpdateLevelText(_actualLevelNumber);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMoveLimit(_levels[levelIndex].moveLimit);
            GameManager.Instance.InitializeGame();
        }

        ClearExistingCubes();

        StartCoroutine(SpawnCubesWithDelay());//Yeni küpleri spawn etmeden bekle

        //Set remaining cubes
        _remainingCubes = _levels[_currentLevelIndex].Cubes.Count;
    }

    private IEnumerator SpawnCubesWithDelay()
    {
        yield return new WaitForEndOfFrame();

        // 1) Sahnedeki eski küpleri temizle
        ClearExistingCubes();

        // 2) Þu anki LevelData’yý al
        var data = _levels[_currentLevelIndex];

        float offsetX = -((data.columns - 1) * 0.5f * _cellSize);
        float offsetY = -((data.rows - 1) * 0.5f * _cellSize);
        // 3) Listede kayýtlý her CubeData için spawn et
        foreach (var cd in data.Cubes)
        {
            // cd.GridPosition kullanarak world pozisyonunu hesapla:
            Vector3 worldPos = new Vector3(
                offsetX + cd.GridPosition.x * _cellSize,
                offsetY + cd.GridPosition.y * _cellSize,
                14f
            );
            CubeFactory.Instance.SpawnCube(cd, worldPos);
        }

        // 4) Kalan küp sayýsýný güncelle (opsiyonel)
        _remainingCubes = data.Cubes.Count;
    }


    private void ClearExistingCubes()
    {
        var cubes = Object.FindObjectsByType<CubeController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (var cube in cubes)
        {
            CubeFactory.Instance.ReleaseCube(cube);
        }
    }
    public void NextLevel()
    {

        // Mevcut level index'i artýr
        _currentLevelIndex++;
        _actualLevelNumber++;

        // Eðer son seviyeye ulaþtýysak, baþa dön
        if (_currentLevelIndex >= _levels.Length)
        {
            _currentLevelIndex = 0;
        }

        Debug.Log($"(Array index: {_currentLevelIndex})");

        LoadLevel(_currentLevelIndex);
    }

    public void RestartLevel()
    {
        LoadLevel(_currentLevelIndex);
    }

    private void HandleMoveChanged(int newMoves)
    {
        //Debug.Log(newMoves);
        //_uiManager.UpdateMovesRemaining(newMoves);
    }

    private void HandleCubeCleared(CubeController cube)
    {
        _remainingCubes--;
        Debug.Log(_remainingCubes);
        Debug.Log(_remainingCubes);

        //uiManager.UpdateCubesRemaining(remainingCubes);

        if (_remainingCubes <= 0)
        {
            Debug.Log("sahne tamamlandý");
            EventManager.RaiseLevelComplete();
        }
    }

    private void HandleLevelFailed()
    {
        //uiManager.ShowLevelFailPanel();
    }
    private void HandleLevelCompleted()
    {
        StartCoroutine(LoadNextLevelWithDelay());
        //_uiManager.ShowLevelCompletePanel();
    }
    private IEnumerator LoadNextLevelWithDelay()
    {
        yield return new WaitForSeconds(0.2f);
        NextLevel();
    }
}
