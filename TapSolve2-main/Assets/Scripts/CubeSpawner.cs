using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;

    private void Start()
    {
        SpawnGrid();
    }

    private void SpawnGrid()
    {
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                // Rastgele yön seç
                Direction direction = (Direction)Random.Range(0, 4);

                // Renk belirle
                Color color = GetColorForDirection(direction);

                // Pozisyonu hesapla
                Vector3 pos = GridManager.Instance.GetWorldPosition(new Vector2Int(x, y));

                // Küpü oluþtur
                GameObject cubeObj = Instantiate(cubePrefab, pos, Quaternion.identity);

                // CubeController'a CubeData ver
                CubeController controller = cubeObj.GetComponent<CubeController>();
                CubeData data = new CubeData
                {
                    Direction = direction,
                    Color = color
                };
                controller.Initialize(data);
            }
        }
    }

    private Color GetColorForDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Color.blue;
            case Direction.Down: return Color.red;
            case Direction.Right: return Color.grey;
            case Direction.Left: return Color.green;
            default: return Color.white;
        }
    }
}
