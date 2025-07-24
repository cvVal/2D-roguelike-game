using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages board generation and provides grid-world coordinate conversion utilities.
/// </summary>
public class BoardManager : MonoBehaviour
{
    [System.Serializable]
    public class CellData
    {
        public bool Passable;
    }

    [Tooltip("Width of the board in grid cells")]
    public int Width;
    
    [Tooltip("Height of the board in grid cells")]
    public int Height;
    
    [Tooltip("Ground tiles for interior placement")]
    public Tile[] GroundTiles;
    
    [Tooltip("Wall tiles for perimeter placement")]
    public Tile[] WallTiles;

    private CellData[,] m_BoardData;
    private Grid m_Grid;
    private Tilemap m_Tilemap;

    /// <summary>
    /// Initializes board components and generates the level layout.
    /// </summary>
    public void Init()
    {
        InitializeComponents();
        GenerateBoard();
    }

    /// <summary>
    /// Converts cell coordinates to world position.
    /// </summary>
    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    /// <summary>
    /// Gets cell data with bounds checking.
    /// </summary>
    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width || cellIndex.y < 0 || cellIndex.y >= Height)
            return null;
            
        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    private void InitializeComponents()
    {
        m_Grid = GetComponentInChildren<Grid>();
        m_Tilemap = GetComponentInChildren<Tilemap>();

        if (m_Grid == null)
        {
            Debug.LogError("Grid component not found in children.");
            return;
        }

        if (m_Tilemap == null)
        {
            Debug.LogError("Tilemap component not found in children.");
            return;
        }

        m_BoardData = new CellData[Width, Height];
    }

    private void GenerateBoard()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                m_BoardData[x, y] = new CellData();
                Tile tileToPlace;

                // Place walls on perimeter, ground tiles inside
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    tileToPlace = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tileToPlace = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tileToPlace);
            }
        }
    }
}
