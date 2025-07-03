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

    private SmartCubeSpawner _cubeSpawner;
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
        _cubeSpawner = FindFirstObjectByType<SmartCubeSpawner>(); // Updated method  
        //_uiManager = FindFirstObjectByType<UIManager>(); // Updated method  

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

        if (GridManager.Instance != null)
        {
            GridManager.Instance.UpdateGridSize(data.rows, data.columns);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMoveLimit(data.rows * data.columns + 4);
            GameManager.Instance.InitializeGame();
        }

        ClearExistingCubes();

        StartCoroutine(SpawnCubesWithDelay());//Yeni küpleri spawn etmeden bekle

        //Set remaining cubes
        _remainingCubes = data.rows * data.columns;
    }

    private IEnumerator SpawnCubesWithDelay()
    {
        yield return new WaitForEndOfFrame(); // Grid ayarlarýnýn uygulanmasý için bekle

        if (_cubeSpawner != null)
        {
            _cubeSpawner.SpawnGrid();
        }
        else
        {
            Debug.LogError("CubeSpawner is null!");
        }
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

        Debug.Log($"Loading level {_currentLevelIndex + 1} (Array index: {_currentLevelIndex})");

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
