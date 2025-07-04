



////iþlevsiz olacak





//using UnityEngine;
//using System;
//using System.Collections.Generic;
//using System.Linq;

///// <summary>
///// SmartCubeSpawner: Çýkmaz döngüleri ve zýt yön çarpýþmalarýný engeller
///// </summary>
//public class SmartCubeSpawner : MonoBehaviour
//{
//    private const int MAX_ATTEMPTS = 20;
//    private const int MAX_BACKTRACK_ATTEMPTS = 50;

//    /// <summary>
//    /// Ana grid spawn metodu
//    /// </summary>
//    public void SpawnGrid()
//    {
//        var gm = GridManager.Instance;
//        var cfm = CubeFactory.Instance;

//        if (gm == null || cfm == null)
//        {
//            Debug.LogError("GridManager veya CubeFactory bulunamadý!");
//            return;
//        }

//        int cols = gm.Columns;
//        int rows = gm.Rows;
//        Direction[,] gridDirs = null;
//        bool success = false;

//        // Backtracking ile daha güvenilir grid oluþturma
//        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
//        {
//            gridDirs = new Direction[cols, rows];
//            if (GenerateRowColSafeGrid(gridDirs, cols, rows))
//            {
//                success = true;
//                Debug.Log($"Grid baþarýyla oluþturuldu! Deneme: {attempt + 1}");
//                break;
//            }
//        }

//        if (!success)
//        {
//            Debug.LogWarning($"Mükemmel grid oluþturulamadý, basit versiyon kullanýlýyor...");
//            gridDirs = GenerateSimpleGrid(cols, rows);
//        }

//        // Grid'i spawn et
//        int spawned = 0;
//        for (int y = 0; y < rows; y++)
//        {
//            for (int x = 0; x < cols; x++)
//            {
//                Direction dir = gridDirs[x, y];
//                Color color = GetColorForDirection(dir);
//                var data = new CubeData { Direction = dir, Color = color };
//                Vector3 pos = gm.GetWorldPosition(new Vector2Int(x, y));

//                if (cfm.SpawnCube(data, pos) != null)
//                    spawned++;
//            }
//        }

//        Debug.Log($"Smart spawn completed: {spawned}/{cols * rows} cubes.");
//    }

//    /// <summary>
//    /// Satýr ve sütun bazlý kýsýtlamalarla yön atamasý yapar - backtracking ile
//    /// </summary>
//    private bool GenerateRowColSafeGrid(Direction[,] grid, int cols, int rows)
//    {
//        return SolveGridRecursive(grid, 0, 0, cols, rows, 0);
//    }

//    /// <summary>
//    /// Recursive backtracking algoritmasý
//    /// </summary>
//    private bool SolveGridRecursive(Direction[,] grid, int x, int y, int cols, int rows, int attempts)
//    {
//        // Çok fazla deneme yapýldýysa çýk
//        if (attempts > MAX_BACKTRACK_ATTEMPTS) return false;

//        // Tüm hücreler doldurulduysa baþarýlý
//        if (y >= rows) return true;

//        // Sonraki hücreye geçiþ koordinatlarý
//        int nextX = (x + 1) % cols;
//        int nextY = (nextX == 0) ? y + 1 : y;

//        // Bu hücre için geçerli yönleri bul
//        var validDirections = GetValidDirections(grid, x, y, cols, rows);

//        // Eðer geçerli yön yoksa geri dön
//        if (validDirections.Count == 0) return false;

//        // Yönleri karýþtýr (randomize)
//        var shuffledDirections = validDirections.OrderBy(d => UnityEngine.Random.value).ToList();

//        // Her geçerli yön için dene
//        foreach (Direction dir in shuffledDirections)
//        {
//            grid[x, y] = dir;

//            // Bu atama ile döngü oluþmuyor mu kontrol et
//            if (!HasAnyCycle(grid, cols, rows))
//            {
//                // Sonraki hücreye geç
//                if (SolveGridRecursive(grid, nextX, nextY, cols, rows, attempts + 1))
//                {
//                    return true;
//                }
//            }
//        }

//        // Hiçbir yön çalýþmadýysa bu hücreyi sýfýrla ve geri dön
//        grid[x, y] = Direction.Up; // Default deðer
//        return false;
//    }

//    /// <summary>
//    /// Belirli bir hücre için geçerli yönleri döndürür
//    /// </summary>
//    private List<Direction> GetValidDirections(Direction[,] grid, int x, int y, int cols, int rows)
//    {
//        var validDirs = new List<Direction>();
//        var allDirections = new Direction[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

//        foreach (Direction dir in allDirections)
//        {
//            if (IsDirectionValid(grid, x, y, dir, cols, rows))
//            {
//                validDirs.Add(dir);
//            }
//        }

//        return validDirs;
//    }

//    /// <summary>
//    /// Bir yönün geçerli olup olmadýðýný kontrol eder
//    /// </summary>
//    private bool IsDirectionValid(Direction[,] grid, int x, int y, Direction dir, int cols, int rows)
//    {
//        // 1. Ayný satýrda zýt yönler var mý kontrol et
//        if (dir == Direction.Left || dir == Direction.Right)
//        {
//            for (int checkX = 0; checkX < cols; checkX++)
//            {
//                if (checkX == x) continue; // Kendisi deðil

//                Direction existingDir = grid[checkX, y];
//                if ((dir == Direction.Left && existingDir == Direction.Right) ||
//                    (dir == Direction.Right && existingDir == Direction.Left))
//                {
//                    return false; // Zýt yön bulundu
//                }
//            }
//        }

//        // 2. Ayný sütunda zýt yönler var mý kontrol et
//        if (dir == Direction.Up || dir == Direction.Down)
//        {
//            for (int checkY = 0; checkY < rows; checkY++)
//            {
//                if (checkY == y) continue; // Kendisi deðil

//                Direction existingDir = grid[x, checkY];
//                if ((dir == Direction.Up && existingDir == Direction.Down) ||
//                    (dir == Direction.Down && existingDir == Direction.Up))
//                {
//                    return false; // Zýt yön bulundu
//                }
//            }
//        }

//        // 3. Grid sýnýrlarýný kontrol et
//        Vector2Int nextPos = GetNeighborCoords(x, y, dir);
//        if (nextPos.x < 0 || nextPos.x >= cols || nextPos.y < 0 || nextPos.y >= rows)
//        {
//            // Sýnýr dýþýna çýkýyor, bu durumda geçerli (dead end)
//            return true;
//        }

//        return true;
//    }

//    /// <summary>
//    /// Global döngü (cycle) var mý kontrol eder
//    /// </summary>
//    private bool HasAnyCycle(Direction[,] grid, int cols, int rows)
//    {
//        // Sadece dolu hücreler için kontrol yap
//        for (int x = 0; x < cols; x++)
//        {
//            for (int y = 0; y < rows; y++)
//            {
//                if (DetectCycleFrom(x, y, grid, cols, rows))
//                {
//                    return true;
//                }
//            }
//        }
//        return false;
//    }

//    /// <summary>
//    /// Belirli bir noktadan döngü tespiti yapar
//    /// </summary>
//    private bool DetectCycleFrom(int startX, int startY, Direction[,] grid, int cols, int rows)
//    {
//        var visited = new HashSet<Vector2Int>();
//        var currentPos = new Vector2Int(startX, startY);

//        // Maksimum grid boyutu kadar adým at, eðer daha fazla adým atarsa döngü var demektir
//        int maxSteps = cols * rows + 1;
//        int steps = 0;

//        while (steps < maxSteps)
//        {
//            // Daha önce ziyaret edildi mi?
//            if (visited.Contains(currentPos))
//            {
//                return true; // Döngü bulundu
//            }

//            visited.Add(currentPos);

//            // Sonraki pozisyonu hesapla
//            Direction currentDir = grid[currentPos.x, currentPos.y];
//            Vector2Int nextPos = GetNeighborCoords(currentPos.x, currentPos.y, currentDir);

//            // Sýnýr dýþýna çýkýyorsa döngü yok
//            if (nextPos.x < 0 || nextPos.x >= cols || nextPos.y < 0 || nextPos.y >= rows)
//            {
//                return false;
//            }

//            currentPos = nextPos;
//            steps++;
//        }

//        return true; // Çok uzun path, muhtemelen döngü
//    }

//    /// <summary>
//    /// Basit grid oluþturur (fallback)
//    /// </summary>
//    private Direction[,] GenerateSimpleGrid(int cols, int rows)
//    {
//        var grid = new Direction[cols, rows];
//        var directions = new Direction[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

//        for (int y = 0; y < rows; y++)
//        {
//            for (int x = 0; x < cols; x++)
//            {
//                // Rastgele yön ata, ama basit kurallara uy
//                Direction dir = directions[UnityEngine.Random.Range(0, directions.Length)];

//                // Kenar hücreler için sýnýr dýþýna çýkan yönleri tercih et
//                if (x == 0 && UnityEngine.Random.value < 0.5f) dir = Direction.Left;
//                else if (x == cols - 1 && UnityEngine.Random.value < 0.5f) dir = Direction.Right;
//                else if (y == 0 && UnityEngine.Random.value < 0.5f) dir = Direction.Down;
//                else if (y == rows - 1 && UnityEngine.Random.value < 0.5f) dir = Direction.Up;

//                grid[x, y] = dir;
//            }
//        }

//        return grid;
//    }

//    /// <summary>
//    /// Grid'den cube'larý spawn eder - artýk kullanýlmýyor, SpawnGrid içinde yapýlýyor
//    /// </summary>
//    //private void SpawnCubesFromGrid(Direction[,] grid, GridManager gm, CubeFactory cfm)
//    //{
//    //    // Bu metot artýk SpawnGrid içinde direkt yapýlýyor
//    //    // Uyumluluk için býrakýldý
//    //}

//    /// <summary>
//    /// Bir yöndeki komþu hücre koordinatýný hesaplar
//    /// </summary>
//    private Vector2Int GetNeighborCoords(int x, int y, Direction d)
//    {
//        switch (d)
//        {
//            case Direction.Up: return new Vector2Int(x, y + 1);
//            case Direction.Down: return new Vector2Int(x, y - 1);
//            case Direction.Left: return new Vector2Int(x - 1, y);
//            case Direction.Right: return new Vector2Int(x + 1, y);
//            default: return new Vector2Int(x, y);
//        }
//    }

//    /// <summary>
//    /// Yöne göre renk döndürür
//    /// </summary>
//    private Color GetColorForDirection(Direction dir)
//    {
//        switch (dir)
//        {
//            case Direction.Up: return Color.blue;
//            case Direction.Down: return Color.black;
//            case Direction.Left: return Color.green;
//            case Direction.Right: return Color.grey;
//            default: return Color.white;
//        }
//    }

//    /// <summary>
//    /// Grid bilgilerini log'lar
//    /// </summary>
//    private void LogGridInfo(Direction[,] grid)
//    {
//        int cols = grid.GetLength(0);
//        int rows = grid.GetLength(1);

//        // Her satýr ve sütunda zýt yönlerin olup olmadýðýný kontrol et
//        int conflictRows = 0, conflictCols = 0;

//        for (int y = 0; y < rows; y++)
//        {
//            bool hasLeft = false, hasRight = false;
//            for (int x = 0; x < cols; x++)
//            {
//                if (grid[x, y] == Direction.Left) hasLeft = true;
//                if (grid[x, y] == Direction.Right) hasRight = true;
//            }
//            if (hasLeft && hasRight) conflictRows++;
//        }

//        for (int x = 0; x < cols; x++)
//        {
//            bool hasUp = false, hasDown = false;
//            for (int y = 0; y < rows; y++)
//            {
//                if (grid[x, y] == Direction.Up) hasUp = true;
//                if (grid[x, y] == Direction.Down) hasDown = true;
//            }
//            if (hasUp && hasDown) conflictCols++;
//        }

//        Debug.Log($"Grid analizi - Çakýþan satýrlar: {conflictRows}/{rows}, Çakýþan sütunlar: {conflictCols}/{cols}");
//    }
//}