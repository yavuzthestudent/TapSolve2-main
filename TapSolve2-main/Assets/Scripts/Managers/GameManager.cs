using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        LevelManager.Instance.LoadLevel(0);
    }

    public void NextLevel()
    {
        LevelManager.Instance.LoadNext();
    }

    public void RestartLevel()
    {
        LevelManager.Instance.ReloadCurrent();
    }
}
