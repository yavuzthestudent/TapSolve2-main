using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private LevelManager _levelManager;

    private void Awake()
    {
        Debug.Log("calisti");
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void NextLevel()
    {
        _levelManager.LoadNext();
    }

    public void RestartLevel()
    {
        _levelManager.ReloadCurrent();
    }
}
