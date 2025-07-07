using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private LevelData[] levels;

    [SerializeField] private Level _levelView;

    private int _currentIndex = 0;           
    private int _displayedLevelNumber = 1;     

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        LoadLevel(_currentIndex);
    }

    public void LoadLevel(int index)
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("LevelData dizisi boþ veya atanmamýþ!");
            return;
        }

        _currentIndex = index % levels.Length;

        EventManager.RaiseLevelNumberChanged(_displayedLevelNumber);

        // Seçili leveli yükle
        _levelView.LoadLevel(levels[_currentIndex]);
    }

    public void ReloadCurrent()
    {
        EventManager.RaiseLevelNumberChanged(_displayedLevelNumber);
        _levelView.LoadLevel(levels[_currentIndex]);
    }

    public void LoadNext()
    {
        _currentIndex++;
        _displayedLevelNumber++;
        LoadLevel(_currentIndex);
    }
}
