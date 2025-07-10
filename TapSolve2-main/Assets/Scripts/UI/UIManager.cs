using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _movesText;
    [SerializeField] private GameObject _failedScreen;
    [SerializeField] private GameObject _replayButton; //retry button deðil bu oyun içinde olan, pasifleþtirilecek kaybedildiðinde
    [SerializeField] private GameObject _successScreen;
    [SerializeField] private GameObject _progressPopUp;

    private Vector3 _endScreenScale = new Vector3(5.25f, 6.70f, 1f);
    private float _animationDuration = 1f;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject); return; 
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        EventManager.OnMoveChanged += UpdateMovesRemaining;
        EventManager.OnLevelCompleted += OnLevelCleared;
        EventManager.OnLevelFail += OnLevelFailed;
        EventManager.OnLevelNumberChanged += UpdateLevelText;
        EventManager.OnProgress += OnProgressPopUp;
    }

    private void OnDisable()
    {
        EventManager.OnMoveChanged -= UpdateMovesRemaining;
        EventManager.OnLevelCompleted -= OnLevelCleared;
        EventManager.OnLevelFail -= OnLevelFailed;
        EventManager.OnLevelNumberChanged -= UpdateLevelText;
        EventManager.OnProgress -= OnProgressPopUp;
    }

    public void LoadGame()
    {
        Debug.Log("Game loaded");
        SceneManager.LoadScene("SampleScene");
    }

    public void UpdateLevelText(int levelNumber)
    {
        Debug.Log("Level number updated: " + levelNumber);
        _levelText.text = $"Level {levelNumber}";
    }

    private void UpdateMovesRemaining(int movesLeft)
    {
        _movesText.text = $"Moves: {movesLeft}";
    }

    public void OnReplayPressed()
    {
        _progressPopUp.SetActive(false);
        _failedScreen.SetActive(false);
        _replayButton.SetActive(true);

        GameManager.Instance.RestartLevel();
        if(GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance is null!");
            return;
        }
    }

    public void OnNextButtonPressed()
    {
        _successScreen.SetActive(false);
        _replayButton.SetActive(true);
        // Sonraki seviyeye geç
        GameManager.Instance.NextLevel();
    }

    public void OnProgressButtonPressed()
    {
        _progressPopUp.SetActive(false);
        _replayButton.SetActive(true);
    }

    private void OnLevelCleared()
    {
        ShowEndScreen(_successScreen);
    }

    private void OnLevelFailed()
    {
        ShowEndScreen(_failedScreen);
    }

    private void OnProgressPopUp()
    {
        ShowEndScreen(_progressPopUp);
    }

    private void ShowEndScreen(GameObject screen)
    {
        // Replay tuþunu kapat
        _replayButton.SetActive(false);

        screen.SetActive(true);
        screen.transform.localScale = Vector3.zero;

        screen.transform
              .DOScale(_endScreenScale, _animationDuration)
              .SetEase(Ease.OutBack);
    }
}
