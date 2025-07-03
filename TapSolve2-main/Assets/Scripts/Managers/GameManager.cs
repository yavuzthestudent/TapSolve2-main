using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [field: SerializeField] public int MovesLimit { get; set; }

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

    public void SetMoveLimit(int limit)
    {
        MovesLimit = limit;
    }

    public void InitializeGame()
    {
        EventManager.RaiseMoveChanged(MovesLimit);
    }

    public void UseMove()
    {
        // 1) Decrement
        MovesLimit--;
        // 2) Update UI
        EventManager.RaiseMoveChanged(MovesLimit);
        // 3) If we just hit zero or below, game over right now
        if (MovesLimit <= 0)
        {
            EventManager.RaiseLevelFail();
        }
    }

}
