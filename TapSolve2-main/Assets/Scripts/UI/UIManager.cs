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
    [SerializeField] private GameObject _replayButton; //retry button de�il bu oyun i�inde olan, pasifle�tirilecek kaybedildi�inde
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


    private void OnLevelCleared()
    {
        // �stersen �Next� butonu ��kart ya da bir pencere a�
        // O ana kadar Replay de i�levini korusun
    }

    private void OnLevelFailed()
    {
        Debug.Log("Level Failed!");
        //_failedScreen.SetActive(true);
        PlayLoseAnimation();
        _replayButton.SetActive(false);
    }
    private void PlayLoseAnimation()
    {
        // Paneli g�r�n�r yap
        _failedScreen.SetActive(true);

        // Ba�lang�� �l�e�i
        _failedScreen.transform.localScale = Vector3.zero;

        // Sequence ile pop-in
        Sequence seq = DOTween.Sequence();
        seq.Append(_failedScreen.transform
            // X=5.25, Y=6.7, Z=1 olarak �l�ekle
            .DOScale(new Vector3(5.25f, 6.70f, 1f), 1f)
            .SetEase(Ease.OutBack)
        );
    }
}
