using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnerController : MonoBehaviour
{
    [SerializeField]
    private Tilemap topLayerTilemap;
    [SerializeField]
    private Tilemap boardTilemap;
    [SerializeField]
    private Sprite markerSprite;
    [SerializeField]
    private Color[] tileColors;
    [SerializeField]
    private float dragThreshold = 0.5f;

    private TilePoolManager _tilePoolManager;
    private Vector3Int _spawnerCellPosition;
    private Grid _grid;
    private Camera _mainCamera;
    private bool _isSpawning;
    private bool _isDragging = false;

    public bool IsDragging => _isDragging;
    
    public void Init(Grid grid, TilePoolManager tilePoolManager)
    {
        _grid = grid;
        _tilePoolManager = tilePoolManager;
        _mainCamera = Camera.main;
        FindCenterOrClosestCell();
    }

    public void OnUpdate()
    {
        HandleSpawnerMovement();

        if (_isSpawning)
        {
            SpawnTileInSpiral();
        }
    }

    public void StartSpawning()
    {
        _isSpawning = true;
    }

    public void StopSpawning()
    {
        _isSpawning = false;
    }

    private void HandleSpawnerMovement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPosition = GetMouseWorldPosition();
            if (Vector2.Distance(mouseWorldPosition, transform.position) < dragThreshold)
            {
                _isDragging = true;
            }
        }

        if (_isDragging)
        {
            if (Input.GetMouseButton(0))
            {
                UpdateSpawnerDragPosition();
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
                SnapSpawnerToNearestValidCell();
            }
        }
    }

    private Vector2 GetMouseWorldPosition() => _mainCamera.ScreenToWorldPoint(Input.mousePosition);

    private void UpdateSpawnerDragPosition() => transform.position = GetMouseWorldPosition();

    private void SnapSpawnerToNearestValidCell()
    {
        Vector3Int nearestCell = _grid.GetNearestValidCell(CalculateCellPositionFromWorld(transform.position));
        SetSpawnerPosition(nearestCell);
    }

    private void FindCenterOrClosestCell()
    {
        SetSpawnerPosition(_grid.GetCenterOrClosestUnblockedCell());
    }

    private void SetSpawnerPosition(Vector3Int cellPosition)
    {
        _spawnerCellPosition = cellPosition;
        transform.position = CalculateWorldPosition(cellPosition);
    }

    private Vector3 CalculateWorldPosition(Vector3Int cellPosition)
    {
        Vector2 cellSize = boardTilemap.cellSize;
        Vector2 cellHalfSize = cellSize * 0.5f;
        Vector3 cellCenterWorldPosition = new Vector3(cellPosition.x + cellHalfSize.x, cellPosition.y + cellHalfSize.y, 0);
        Vector3 centerOffset = new Vector3(_grid.Size.x * cellSize.x * 0.5f, _grid.Size.y * cellSize.y * 0.5f, 0);
        return cellCenterWorldPosition - centerOffset;
    }

    private void SpawnTileInSpiral()
    {
        var nextCell = _grid.GetNextSpiralCell(_spawnerCellPosition);
        if (nextCell.HasValue && _grid.IsCellValid(nextCell.Value))
        {
            SpawnTile(nextCell.Value);
        }
    }

    private void SpawnTile(Vector3Int cell)
    {
        Vector3 spawnPosition = CalculateWorldPosition(cell);
        (Color tileColor, int index) = GetRandomTileColor();
        _grid.BlockCell(cell, index);
        StartCoroutine(AnimateTileSpawn(spawnPosition, cell, tileColor));
    }

    private (Color, int) GetRandomTileColor()
    {
        int index = Random.Range(0, tileColors.Length);
        return (tileColors[index], index);
    }

    IEnumerator AnimateTileSpawn(Vector3 targetPosition, Vector3Int cellPosition, Color tileColor)
    {
        GameObject tempTile = _tilePoolManager.GetPooledTile();
        SetUpTemporaryTile(tempTile, tileColor);

        Vector3 startPosition = transform.position;
        yield return AnimateTileMovement(tempTile, startPosition, targetPosition);

        PlaceTileInTilemap(cellPosition, tileColor);
        _tilePoolManager.ReturnPooledTile(tempTile);
    }

    private void SetUpTemporaryTile(GameObject tempTile, Color tileColor)
    {
        tempTile.SetActive(true);
        SpriteRenderer renderer = tempTile.GetComponent<SpriteRenderer>();
        renderer.color = tileColor;
        renderer.sortingOrder = 2;
    }

    private IEnumerator AnimateTileMovement(GameObject tile, Vector3 start, Vector3 end)
    {
        float speed = 10f;
        float duration = Vector3.Distance(start, end) / speed;
        for (float t = 0; t < 1; t += Time.deltaTime / duration)
        {
            tile.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }

    private void PlaceTileInTilemap(Vector3Int cellPosition, Color tileColor)
    {
        Tile tile = CreateColoredTile(tileColor);
        Vector3Int tilePosition = new Vector3Int(cellPosition.x - _grid.Size.x / 2, cellPosition.y - _grid.Size.y / 2, 0);
        topLayerTilemap.SetTile(tilePosition, tile);
    }

    private Tile CreateColoredTile(Color color)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = markerSprite;
        tile.color = color;
        return tile;
    }

    private Vector3Int CalculateCellPositionFromWorld(Vector3 worldPosition)
    {
        Vector2 cellSize = boardTilemap.cellSize;
        Vector3 centerOffset = new Vector3(-_grid.Size.x * cellSize.x / 2, -_grid.Size.y * cellSize.y / 2, 0);
        Vector3 offsetPosition = worldPosition - centerOffset;
        int cellX = Mathf.FloorToInt(offsetPosition.x / cellSize.x);
        int cellY = Mathf.FloorToInt(offsetPosition.y / cellSize.y);
        return new Vector3Int(cellX, cellY, 0);
    }
}
