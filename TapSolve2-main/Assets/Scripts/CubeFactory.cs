using UnityEngine;

public class CubeFactory : MonoBehaviour
{
    public static CubeFactory Instance { get; private set; }

    [SerializeField] private GameObject _cubePrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public CubeController SpawnCube(CubeData data, Vector3 position)
    {
        GameObject go = Instantiate(_cubePrefab, position, Quaternion.identity);
        CubeController controller = go.GetComponent<CubeController>();
        controller.Initialize(data);
        return controller;
    }
    void Start()
    {
        // Test verisi
        CubeData testData = new CubeData
        {
            Direction = Direction.Up,
            Color = Color.blue,
            //GridPosition = Vector2Int.zero
        };
        CubeFactory.Instance.SpawnCube(testData, Vector3.zero);
    }


}
