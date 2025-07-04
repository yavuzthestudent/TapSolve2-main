



////i�levsiz olacak





//using UnityEngine;
//using System;
//using System.Collections.Generic;
//using System.Linq;

///// <summary>
///// SmartCubeSpawner: ��kmaz d�ng�leri ve z�t y�n �arp��malar�n� engeller
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
//            Debug.LogError("GridManager veya CubeFactory bulunamad�!");
//            return;
//        }

//        int cols = gm.Columns;
//        int rows = gm.Rows;
//        Direction[,] gridDirs = null;
//        bool success = false;

//        // Backtracking ile daha g�venilir grid olu�turma
//        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
//        {
//            gridDirs = new Direction[cols, rows];
//            if (GenerateRowColSafeGrid(gridDirs, cols, rows))
//            {
//                success = true;
//                Debug.Log($"Grid ba�ar�yla olu�turuldu! Deneme: {attempt + 1}");
//                break;
//            }
//        }

//        if (!success)
//        {
//            Debug.LogWarning($"M�kemmel grid olu�turulamad�, basit versiyon kullan�l�yor...");
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
//    /// Sat�r ve s�tun bazl� k�s�tlamalarla y�n atamas� yapar - backtracking ile
//    /// </summary>
//    private bool GenerateRowColSafeGrid(Direction[,] grid, int cols, int rows)
//    {
//        return SolveGridRecursive(grid, 0, 0, cols, rows, 0);
//    }

//    /// <summary>
//    /// Recursive backtracking algoritmas�
//    /// </summary>
//    private bool SolveGridRecursive(Direction[,] grid, int x, int y, int cols, int rows, int attempts)
//    {
//        // �ok fazla deneme yap�ld�ysa ��k
//        if (attempts > MAX_BACKTRACK_ATTEMPTS) return false;

//        // T�m h�creler doldurulduysa ba�ar�l�
//        if (y >= rows) return true;

//        // Sonraki h�creye ge�i� koordinatlar�
//        int nextX = (x + 1) % cols;
//        int nextY = (nextX == 0) ? y + 1 : y;

//        // Bu h�cre i�in ge�erli y�nleri bul
//        var validDirections = GetValidDirections(grid, x, y, cols, rows);

//        // E�er ge�erli y�n yoksa geri d�n
//        if (validDirections.Count == 0) return false;

//        // Y�nleri kar��t�r (randomize)
//        var shuffledDirections = validDirections.OrderBy(d => UnityEngine.Random.value).ToList();

//        // Her ge�erli y�n i�in dene
//        foreach (Direction dir in shuffledDirections)
//        {
//            grid[x, y] = dir;

//            // Bu atama ile d�ng� olu�muyor mu kontrol et
//            if (!HasAnyCycle(grid, cols, rows))
//            {
//                // Sonraki h�creye ge�
//                if (SolveGridRecursive(grid, nextX, nextY, cols, rows, attempts + 1))
//                {
//                    return true;
//                }
//            }
//        }

//        // Hi�bir y�n �al��mad�ysa bu h�creyi s�f�rla ve geri d�n
//        grid[x, y] = Direction.Up; // Default de�er
//        return false;
//    }

//    /// <summary>
//    /// Belirli bir h�cre i�in ge�erli y�nleri d�nd�r�r
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
//    /// Bir y�n�n ge�erli olup olmad���n� kontrol eder
//    /// </summary>
//    private bool IsDirectionValid(Direction[,] grid, int x, int y, Direction dir, int cols, int rows)
//    {
//        // 1. Ayn� sat�rda z�t y�nler var m� kontrol et
//        if (dir == Direction.Left || dir == Direction.Right)
//        {
//            for (int checkX = 0; checkX < cols; checkX++)
//            {
//                if (checkX == x) continue; // Kendisi de�il

//                Direction existingDir = grid[checkX, y];
//                if ((dir == Direction.Left && existingDir == Direction.Right) ||
//                    (dir == Direction.Right && existingDir == Direction.Left))
//                {
//                    return false; // Z�t y�n bulundu
//                }
//            }
//        }

//        // 2. Ayn� s�tunda z�t y�nler var m� kontrol et
//        if (dir == Direction.Up || dir == Direction.Down)
//        {
//            for (int checkY = 0; checkY < rows; checkY++)
//            {
//                if (checkY == y) continue; // Kendisi de�il

//                Direction existingDir = grid[x, checkY];
//                if ((dir == Direction.Up && existingDir == Direction.Down) ||
//                    (dir == Direction.Down && existingDir == Direction.Up))
//                {
//                    return false; // Z�t y�n bulundu
//                }
//            }
//        }

//        // 3. Grid s�n�rlar�n� kontrol et
//        Vector2Int nextPos = GetNeighborCoords(x, y, dir);
//        if (nextPos.x < 0 || nextPos.x >= cols || nextPos.y < 0 || nextPos.y >= rows)
//        {
//            // S�n�r d���na ��k�yor, bu durumda ge�erli (dead end)
//            return true;
//        }

//        return true;
//    }

//    /// <summary>
//    /// Global d�ng� (cycle) var m� kontrol eder
//    /// </summary>
//    private bool HasAnyCycle(Direction[,] grid, int cols, int rows)
//    {
//        // Sadece dolu h�creler i�in kontrol yap
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
//    /// Belirli bir noktadan d�ng� tespiti yapar
//    /// </summary>
//    private bool DetectCycleFrom(int startX, int startY, Direction[,] grid, int cols, int rows)
//    {
//        var visited = new HashSet<Vector2Int>();
//        var currentPos = new Vector2Int(startX, startY);

//        // Maksimum grid boyutu kadar ad�m at, e�er daha fazla ad�m atarsa d�ng� var demektir
//        int maxSteps = cols * rows + 1;
//        int steps = 0;

//        while (steps < maxSteps)
//        {
//            // Daha �nce ziyaret edildi mi?
//            if (visited.Contains(currentPos))
//            {
//                return true; // D�ng� bulundu
//            }

//            visited.Add(currentPos);

//            // Sonraki pozisyonu hesapla
//            Direction currentDir = grid[currentPos.x, currentPos.y];
//            Vector2Int nextPos = GetNeighborCoords(currentPos.x, currentPos.y, currentDir);

//            // S�n�r d���na ��k�yorsa d�ng� yok
//            if (nextPos.x < 0 || nextPos.x >= cols || nextPos.y < 0 || nextPos.y >= rows)
//            {
//                return false;
//            }

//            currentPos = nextPos;
//            steps++;
//        }

//        return true; // �ok uzun path, muhtemelen d�ng�
//    }

//    /// <summary>
//    /// Basit grid olu�turur (fallback)
//    /// </summary>
//    private Direction[,] GenerateSimpleGrid(int cols, int rows)
//    {
//        var grid = new Direction[cols, rows];
//        var directions = new Direction[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

//        for (int y = 0; y < rows; y++)
//        {
//            for (int x = 0; x < cols; x++)
//            {
//                // Rastgele y�n ata, ama basit kurallara uy
//                Direction dir = directions[UnityEngine.Random.Range(0, directions.Length)];

//                // Kenar h�creler i�in s�n�r d���na ��kan y�nleri tercih et
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
//    /// Grid'den cube'lar� spawn eder - art�k kullan�lm�yor, SpawnGrid i�inde yap�l�yor
//    /// </summary>
//    //private void SpawnCubesFromGrid(Direction[,] grid, GridManager gm, CubeFactory cfm)
//    //{
//    //    // Bu metot art�k SpawnGrid i�inde direkt yap�l�yor
//    //    // Uyumluluk i�in b�rak�ld�
//    //}

//    /// <summary>
//    /// Bir y�ndeki kom�u h�cre koordinat�n� hesaplar
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
//    /// Y�ne g�re renk d�nd�r�r
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

//        // Her sat�r ve s�tunda z�t y�nlerin olup olmad���n� kontrol et
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

//        Debug.Log($"Grid analizi - �ak��an sat�rlar: {conflictRows}/{rows}, �ak��an s�tunlar: {conflictCols}/{cols}");
//    }
//}