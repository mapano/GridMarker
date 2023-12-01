using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grid
{
    public Cell[,] Tiles;
    public Vector2Int Size { get; private set; }

    public void Load(string fileName)
    {
        Size = LoadGridSizeFromFile(fileName);
        InitializeTiles();
    }

    private Vector2Int LoadGridSizeFromFile(string file)
    {
        string filePath = Path.Combine(Application.dataPath, file);
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return Vector2Int.zero;
        }

        try
        {
            string[] dimensions = File.ReadAllText(filePath).Split(',');
            if (dimensions.Length != 2) throw new FormatException("File format is incorrect. Ensure it contains two numbers separated by a comma.");

            return new Vector2Int(int.Parse(dimensions[0]), int.Parse(dimensions[1]));
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to read grid size from file: " + ex.Message);
            return Vector2Int.zero;
        }
    }

    private void InitializeTiles()
    {
        Tiles = new Cell[Size.x, Size.y];
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                Tiles[x, y] = new Cell(Random.value < 0.25);
            }
        }
    }

    public IEnumerable<Vector3Int> GetCellsInRadius(Vector3Int center, int radius)
    {
        List<Vector3Int> cellsInRadius = new List<Vector3Int>();
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int cellPosition = new Vector3Int(center.x + x, center.y + y, 0);
                if (IsCellInsideGrid(cellPosition) && Vector3Int.Distance(center, cellPosition) <= radius)
                    cellsInRadius.Add(cellPosition);
            }
        }
        return cellsInRadius;
    }

    public Vector3Int GetCenterOrClosestUnblockedCell()
    {
        Vector3Int center = new Vector3Int(Size.x / 2, Size.y / 2, 0);
        if (!Tiles[center.x, center.y].IsBlocked) return center;

        for (int radius = 1; radius <= Mathf.Max(Size.x, Size.y); radius++)
        {
            foreach (var cell in GetCellsInRadius(center, radius))
            {
                if (IsCellValid(cell)) return cell;
            }
        }

        return Vector3Int.zero; 
    }

    public void ClearNeighbours()
    {
        int rows = Tiles.GetLength(0);
        int cols = Tiles.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (Tiles[i, j].Value >= 0 && IsEqualNeighbor(i, j, Tiles[i, j].Value))
                    Tiles[i, j].MarkedToClear = true;
            }
        }

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (Tiles[i, j].MarkedToClear)
                    Tiles[i, j].Clear();
            }
        }
    }

    private bool IsEqualNeighbor(int row, int col, int value)
    {
        return CheckNeighbor(row, col - 1, value) || CheckNeighbor(row, col + 1, value) ||
               CheckNeighbor(row - 1, col, value) || CheckNeighbor(row + 1, col, value);
    }

    private bool CheckNeighbor(int row, int col, int value)
    {
        return row >= 0 && row < Tiles.GetLength(0) && col >= 0 && col < Tiles.GetLength(1) && Tiles[row, col].Value == value;
    }

    private Vector3Int[] Directions = new Vector3Int[] { Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left };

    private Vector3Int currentSpiralCell;
    private int currentStepSize = 1;
    private int currentDirectionIndex = 0;
    private int stepsTakenInCurrentDirection = 0;
    private bool isSpiralInitialized = false;

    public void ResetSpiral()
    {
        isSpiralInitialized = false;
        currentSpiralCell = Vector3Int.zero;
        currentStepSize = 1;
        currentDirectionIndex = 0;
        stepsTakenInCurrentDirection = 0;
    }

    public Vector3Int? GetNextSpiralCell(Vector3Int startCell)
    {
        if (!isSpiralInitialized)
        {
            currentSpiralCell = startCell;
            isSpiralInitialized = true;
        }

        do
        {
            if (stepsTakenInCurrentDirection < currentStepSize)
            {
                currentSpiralCell += Directions[currentDirectionIndex];
                stepsTakenInCurrentDirection++;
            }
            else
            {
                UpdateDirection();
            }

            if (!IsCellInsideGrid(currentSpiralCell)) return null;
        } while (Tiles[currentSpiralCell.x, currentSpiralCell.y].IsBlocked);

        return currentSpiralCell;
    }

    private void UpdateDirection()
    {
        currentDirectionIndex = (currentDirectionIndex + 1) % 4;
        stepsTakenInCurrentDirection = 0;
        if (currentDirectionIndex % 2 == 0) currentStepSize++;
    }

    private bool IsCellInsideGrid(Vector3Int cellPosition)
    {
        return cellPosition.x >= 0 && cellPosition.x < Size.x && cellPosition.y >= 0 && cellPosition.y < Size.y;
    }

    public bool IsCellValid(Vector3Int cellPosition)
    {
        return cellPosition.x >= 0 && cellPosition.x < Size.x && cellPosition.y >= 0 && cellPosition.y < Size.y && !Tiles[cellPosition.x, cellPosition.y].IsBlocked;
    }

    public void BlockCell(Vector3Int cellPosition, int value)
    {
        if (IsCellValid(cellPosition))
        {
            Tiles[cellPosition.x, cellPosition.y].IsBlocked = true;
            Tiles[cellPosition.x, cellPosition.y].Value = value;
        }
    }

    public Vector3Int GetNearestValidCell(Vector3Int startPosition)
    {
        if (IsCellValid(startPosition)) return startPosition;

        for (int radius = 1; radius <= Mathf.Max(Size.x, Size.y); radius++)
            foreach (var cell in GetCellsInRadius(startPosition, radius))
                if (IsCellValid(cell)) return cell;

        return Vector3Int.zero; 
    }
}

public struct Cell
{
    public bool IsBlocked;
    public bool MarkedToClear;
    public int Value;

    public Cell(bool isBlocked)
    {
        IsBlocked = isBlocked;
        MarkedToClear = false;
        Value = isBlocked ? -2 : -1;
    }

    public bool IsNewRow => Value == -1;

    public void Clear()
    {
        IsBlocked = false;
        MarkedToClear = false;
        Value = -1;
    }
}
