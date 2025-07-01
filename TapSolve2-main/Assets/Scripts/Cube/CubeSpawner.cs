using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public void SpawnGrid()
    {
        for (int x = 0; x < GridManager.Instance.Columns; x++)
        {
            for (int y = 0; y < GridManager.Instance.Rows; y++)
            {
                // Rastgele yön seç
                Direction direction = (Direction)Random.Range(0, 4);

                // Renk belirle
                Color color = GetColorForDirection(direction);


                CubeData data = new CubeData
                {
                    Direction = direction,
                    Color = color
                };

                // Pozisyonu hesapla
                Vector3 pos = GridManager.Instance.GetWorldPosition(new Vector2Int(x, y));

                CubeFactory.Instance.SpawnCube(data, pos);
            }
        }
    }

    private Color GetColorForDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Color.blue;
            case Direction.Down: return Color.black;
            case Direction.Right: return Color.grey;
            case Direction.Left: return Color.green;
            default: return Color.white;
        }
    }
}
