using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [field: SerializeField] public int Rows { get; set; }
    [field: SerializeField] public int Columns { get; set; }
    [SerializeField] private float _cellSize = 1;
    [SerializeField] private Vector3 _origin = Vector3.zero;

    private void Awake()
    {
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

    private void CalculateOrigin()
    {
        float baseY = -2f;
        int baseRows = 4;
        float yOrigin = baseY - (Rows - baseRows) * 0.5f;
        float xOrigin = -((Columns - 1) / 2f);

        _origin = new Vector3(xOrigin, yOrigin, 17f);
    }

    // Rows ve Columns deðiþtiðinde origin'i yeniden hesapla
    public void UpdateGridSize(int rows, int columns)
    {
        Debug.Log($"UpdateGridSize called: {Rows}x{Columns} -> {rows}x{columns}");

        Rows = rows;
        Columns = columns;

        // Origin'i yeniden hesapla
        CalculateOrigin();

        Debug.Log($"New origin calculated: {_origin}");
    }

    private void OnDrawGizmos()
    {
        if (Rows <= 0 || Columns <= 0) return;

        Gizmos.color = Color.yellow;
        for (int x = 0; x < Columns; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                Vector3 pos = GetWorldPosition(new Vector2Int(x, y));
                Gizmos.DrawWireCube(pos, Vector3.one * 0.9f);
            }
        }
    }
}