using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelData[] levels;

    [SerializeField] private Level _levelPrefab;

    private const string PrefsKeyLevelIndex = "SavedLevelIndex";
    private const string PrefsKeyDisplayLevelNumber = "SavedDisplayLevelNumber";

    private int _currentIndex = 0;           
    private int _displayedLevelNumber = 1;
    private Level _levelView;

    private void Awake()
    {
        //Kayýtlýysa kayýtlarý deðiþkenlere ata
        if(PlayerPrefs.HasKey(PrefsKeyLevelIndex))
        {
            _currentIndex = PlayerPrefs.GetInt(PrefsKeyLevelIndex);
            _displayedLevelNumber = PlayerPrefs.GetInt(PrefsKeyDisplayLevelNumber);
        }
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

        // Mevcut level prefab'ýný yok et
        if (_levelView != null)
        {
            // Event'leri temizle
            _levelView.gameObject.SetActive(false);
            Destroy(_levelView.gameObject);
            Debug.Log("Level prefab yok edildi");
        }

        // Mevcut küpleri temizle
        CubeFactory.Instance.ClearAllCubes();

        // Yeni level prefab'ý oluþtur
        _levelView = Instantiate(_levelPrefab);

        // Seçili leveli yükle
        _levelView.LoadLevel(levels[_currentIndex]);

        PlayerPrefs.SetInt(PrefsKeyLevelIndex, _currentIndex);
        PlayerPrefs.SetInt(PrefsKeyDisplayLevelNumber, _displayedLevelNumber);
        PlayerPrefs.Save();
    }

    public void ReloadCurrent()
    {
        if (_levelView != null)
        {
            _levelView.ClearSavedCubes();
        }

        EventManager.RaiseLevelNumberChanged(_displayedLevelNumber);

        if (_levelView != null)
        {
            Destroy(_levelView.gameObject);
            Debug.Log("yok edildi");
        }
        CubeFactory.Instance.ClearAllCubes();
        _levelView = Instantiate(_levelPrefab);
        _levelView.LoadLevel(levels[_currentIndex]);
    }

    public void LoadNext()
    {
        if (_levelView != null)
        {
            _levelView.ClearSavedCubes();
        }

        _currentIndex++;
        _displayedLevelNumber++;
        LoadLevel(_currentIndex);
    }
}
