using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int _moveLimit = 20;

    public int MovesLimit { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        MovesLimit = _moveLimit;
        EventManager.RaiseMoveChanged(MovesLimit);
    }

    public void UseMove()
    {
        if (_moveLimit <= 0)
        {
            EventManager.RaiseLevelFail();
            return;
        }
        _moveLimit--;
        EventManager.RaiseMoveChanged(_moveLimit);
    }
}
