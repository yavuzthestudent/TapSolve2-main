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
    }

    private void OnDisable()
    {
        EventManager.OnMoveChanged -= UpdateMovesRemaining;
        EventManager.OnLevelCompleted -= OnLevelCleared;
        EventManager.OnLevelFail -= OnLevelFailed;
    }

    public void UpdateLevelText(int levelNumber)
    {
        _levelText.text = $"Level {levelNumber}";
    }

    private void UpdateMovesRemaining(int movesLeft)
    {
        _movesText.text = $"Moves: {movesLeft}";
    }

    public void LoadSampleScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnReplayPressed()
    {
        _failedScreen.SetActive(false);
        _replayButton.SetActive(true);

        LevelManager.Instance.RestartLevel();
    }

    public void OnNextButtonPressed()
    {
        _successScreen.SetActive(false);
        _replayButton.SetActive(true);
        // Sonraki seviyeye geç
        LevelManager.Instance.NextLevel();
    }
    private void OnLevelCleared() => ShowEndScreen(_successScreen);

    private void OnLevelFailed() => ShowEndScreen(_failedScreen);

    private void ShowEndScreen(GameObject screen)
    {
        Debug.Log("SHOOWWW END SCREEN");
        // Replay tuþunu kapat
        _replayButton.SetActive(false);

        // Paneli görünür kýl ve baþlangýç ölçeðini sýfýrla
        screen.SetActive(true);
        screen.transform.localScale = Vector3.zero;

        // Tween’i doðrudan transform’a uygulayýp OutBack easing’i kullan
        screen.transform
              .DOScale(_endScreenScale, _animationDuration)
              .SetEase(Ease.OutBack);
    }
}
