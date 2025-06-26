using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private int _rows = 5;
    [SerializeField] private int _columns = 5;
    [SerializeField] private float _cellSize = 1.5f;
    [SerializeField] Vector3 _origin = Vector3.zero;

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
        float z = _origin.z + gridPosition.y * _cellSize;
        return new Vector3(x, _origin.y, z);
    }
}
