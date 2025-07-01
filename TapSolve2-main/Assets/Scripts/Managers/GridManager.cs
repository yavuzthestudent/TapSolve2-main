using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [field: SerializeField] public int Rows { get; private set; }
    [field: SerializeField] public int Columns { get; private set; }
    [SerializeField] private float _cellSize = 1;
    [SerializeField] private Vector3 _origin = Vector3.zero;

    private void Awake()
    {
        float xOrigin = -((Columns - 1) / 2f);
        _origin = new Vector3(xOrigin, -3f, 17f);
        

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        float x = _origin.x + gridPosition.x * _cellSize;
        float y = _origin.y + gridPosition.y * _cellSize;
        return new Vector3(x, y, _origin.z);
    }
}