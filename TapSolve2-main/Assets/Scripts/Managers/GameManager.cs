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
        if (MovesLimit <= 0)
        {
            Debug.Log("Hamle sayýsý bitti!");
            EventManager.RaiseLevelFail();
            return;
        }
        // Hamle sayýsýný azalt
        MovesLimit--;
        EventManager.RaiseMoveChanged(MovesLimit);
    }
}
