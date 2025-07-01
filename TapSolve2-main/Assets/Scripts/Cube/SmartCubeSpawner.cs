using UnityEngine;
using System;
using System.Collections.Generic;

public class SmartCubeSpawner : MonoBehaviour
{
    public void SpawnGrid()
    {
        var gm = GridManager.Instance;
        var cfm = CubeFactory.Instance;
        if (gm == null || cfm == null)
        {
            Debug.LogError("GridManager veya CubeFactory bulunamadý!");
            return;
        }

        int cols = gm.Columns;
        int rows = gm.Rows;

        // 1) Yönleri tutacak dizi  
        Direction[,] gridDirs = new Direction[cols, rows];

        // 2) Satýr/sütun düzeyinde hangi yön atandýysa onu izleyelim  
        bool[] rowHasLeft = new bool[rows];
        bool[] rowHasRight = new bool[rows];
        bool[] colHasUp = new bool[cols];
        bool[] colHasDown = new bool[cols];

        // 3) Yatay satýr bazlý, dikey sütun bazlý kýsýtlamalarla ata  
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var candidates = new List<Direction>(4);

                foreach (Direction d in Enum.GetValues(typeof(Direction)))
                {
                    // Ayný satýrda RIGHT varsa LEFT, LEFT varsa RIGHT yasak  
                    if (d == Direction.Left && rowHasRight[y]) continue;
                    if (d == Direction.Right && rowHasLeft[y]) continue;

                    // Ayný sütunda UP varsa DOWN, DOWN varsa UP yasak  
                    if (d == Direction.Up && colHasDown[x]) continue;
                    if (d == Direction.Down && colHasUp[x]) continue;

                    candidates.Add(d);
                }

                // Eðer kalan aday yoksa (çok nadir), fallback olarak hepsini ekle  
                if (candidates.Count == 0)
                    candidates.AddRange((Direction[])Enum.GetValues(typeof(Direction)));

                // Rastgele seç  
                var chosen = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                gridDirs[x, y] = chosen;

                // Bayraklarý güncelle  
                if (chosen == Direction.Left) rowHasLeft[y] = true;
                if (chosen == Direction.Right) rowHasRight[y] = true;
                if (chosen == Direction.Up) colHasUp[x] = true;
                if (chosen == Direction.Down) colHasDown[x] = true;
            }
        }

        // 4) Spawn iþlemi  
        int spawned = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var dir = gridDirs[x, y];
                var color = GetColorForDirection(dir);
                var data = new CubeData { Direction = dir, Color = color };
                var pos = gm.GetWorldPosition(new Vector2Int(x, y));

                if (cfm.SpawnCube(data, pos) != null)
                    spawned++;
            }
        }

        Debug.Log($"RowCol-safe spawn: {spawned}/{cols * rows} küp yerleþti.");
    }

    private Color GetColorForDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Color.blue;
            case Direction.Down: return Color.black;
            case Direction.Left: return Color.green;
            case Direction.Right: return Color.grey;
            default: return Color.white;
        }
    }
}
