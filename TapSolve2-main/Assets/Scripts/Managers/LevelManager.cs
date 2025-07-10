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
        // Kayýtlýysa kayýtlarý deðiþkenlere ata
        if (PlayerPrefs.HasKey(PrefsKeyLevelIndex))
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

        // Mevcut level prefab'ýný yok et
        DestroyCurrentLevel();

        // Mevcut küpleri temizle
        CubeFactory.Instance.ClearAllCubes();

        // Yeni level yaratmak için ortak metodu kullan
        CreateNewLevel();

        // Event'i son olarak fire et
        EventManager.RaiseLevelNumberChanged(_displayedLevelNumber);

        // Kaydet
        SaveProgress();
    }

    public void ReloadCurrent()
    {
        // Mevcut level'in cube'larýný temizle
        if (_levelView != null)
        {
            _levelView.ClearSavedCubes();
        }

        // Mevcut level'i yok et
        DestroyCurrentLevel();

        // Küpleri temizle
        CubeFactory.Instance.ClearAllCubes();

        // Yeni level yarat
        CreateNewLevel();

        Debug.Log("Level reload edildi");

        // Event'i en son fire et (bu event baþka yerlerde level yaratma tetikliyor olabilir)
        EventManager.RaiseLevelNumberChanged(_displayedLevelNumber);
    }

    public void LoadNext()
    {
        // Mevcut level'in cube'larýný temizle
        if (_levelView != null)
        {
            _levelView.ClearSavedCubes();
        }

        // Index'leri artýr
        _currentIndex++;
        _displayedLevelNumber++;

        // Yeni level'i yükle
        LoadLevel(_currentIndex);
    }

    // Mevcut level'i yok eden ortak metot
    private void DestroyCurrentLevel()
    {
        if (_levelView != null)
        {
            _levelView.gameObject.SetActive(false);
            Destroy(_levelView.gameObject);
            _levelView = null; // Referansý temizle
            Debug.Log("Level prefab yok edildi");
        }
    }

    // Yeni level yaratan ortak metot
    private void CreateNewLevel()
    {
        if (_levelPrefab == null)
        {
            Debug.LogError("Level prefab atanmamýþ!");
            return;
        }

        if (levels == null || _currentIndex >= levels.Length)
        {
            Debug.LogError("Geçersiz level index!");
            return;
        }

        // Yeni level prefab'ýný yarat
        _levelView = Instantiate(_levelPrefab);

        if (_levelView != null)
        {
            // Level data'sýný yükle
            _levelView.LoadLevel(levels[_currentIndex]);
            _levelView.gameObject.SetActive(true);
            Debug.Log("Level prefab yaratýldý ve yüklendi");
        }
        else
        {
            Debug.LogError("Level prefab yaratýlamadý!");
        }
    }

    // Kayýt iþlemi için ortak metot
    private void SaveProgress()
    {
        PlayerPrefs.SetInt(PrefsKeyLevelIndex, _currentIndex);
        PlayerPrefs.SetInt(PrefsKeyDisplayLevelNumber, _displayedLevelNumber);
        PlayerPrefs.Save();
    }
}