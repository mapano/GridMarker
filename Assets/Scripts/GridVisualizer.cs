using UnityEngine;
using UnityEngine.Tilemaps;

public class GridVisualizer : MonoBehaviour
{
    private Grid _grid;
    
    [SerializeField]
    private Sprite tileSprite;
    [SerializeField]
    private Tilemap board;
    [SerializeField]
    private Tilemap topLayer;

    public void Init(Grid grid)
    {
        _grid = grid;
    }

public void ClearTilesInTilemap()
{
    int rows = _grid.Tiles.GetLength(0);
    int cols = _grid.Tiles.GetLength(1);

    for (int i = 0; i < rows; i++)
    {
        for (int j = 0; j < cols; j++)
        {
            Cell tileValue = _grid.Tiles[i, j];

            if (tileValue.Value == -1)
            {
                Vector3Int tilePosition = new Vector3Int(
                    i - _grid.Size.x / 2,
                    j - _grid.Size.y / 2,
                    0
                );

                topLayer.SetTile(tilePosition, null);
            }
        }
    }
}
    
    public void DisplayGrid()
    {
        for (int x = 0; x < _grid.Size.x; x++)
        {
            for (int y = 0; y < _grid.Size.y; y++)
            {
                Tile tile = CreateTile(x, y);
                Vector3Int tilePosition = new Vector3Int(x - _grid.Size.x / 2, y - _grid.Size.y / 2, 0);
                board.SetTile(tilePosition, tile);
            }
        }
        
        int rows = _grid.Tiles.GetLength(0);
        int cols = _grid.Tiles.GetLength(1);
        
        if (rows % 2 != 0)
            transform.position = new Vector2(transform.position.x, transform.position.y - 0.5f);
        if (cols % 2 != 0)
            transform.position = new Vector2(transform.position.x - 0.5f, transform.position.y);
    }

    private Tile CreateTile(int x, int y)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = tileSprite;

        if (_grid.Tiles[x, y].IsBlocked)
        {
            tile.color = Color.black;
        }
        else
        {
            bool isEven = (x + y) % 2 == 0;
            tile.color = isEven ? Color.white : Color.gray; 
        }

        return tile;
    }
}