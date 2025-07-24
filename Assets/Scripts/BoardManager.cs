using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages the game board generation, tile placement, and provides utilities for cell-world conversion.
/// This script handles creating a bordered roguelike level with walls around the perimeter and ground tiles inside.
/// </summary>
public class BoardManager : MonoBehaviour
{
    /// <summary>
    /// Data structure that holds information about each cell in the board grid.
    /// Used to track gameplay-relevant properties like whether a cell can be moved through.
    /// </summary>
    [System.Serializable]
    public class CellData
    {
        /// <summary>
        /// Determines if this cell can be passed through by the player or other entities.
        /// False for walls, true for ground tiles.
        /// </summary>
        public bool Pasable; // Note: Should be "Passable" but keeping current spelling for compatibility
    }

    #region Private Fields
    /// <summary>
    /// 2D array storing gameplay data for each cell in the board grid.
    /// </summary>
    private CellData[,] m_BoardData;
    
    /// <summary>
    /// Reference to the Grid component used for coordinate transformations.
    /// </summary>
    private Grid m_Grid;
    
    /// <summary>
    /// Reference to the Tilemap component where tiles are placed.
    /// </summary>
    private Tilemap m_Tilemap;
    #endregion

    #region Public Inspector Fields
    /// <summary>
    /// Width of the board in grid cells.
    /// </summary>
    [Tooltip("Width of the board in grid cells")]
    public int Width;
    
    /// <summary>
    /// Height of the board in grid cells.
    /// </summary>
    [Tooltip("Height of the board in grid cells")]
    public int Height;
    
    /// <summary>
    /// Array of ground tile assets to randomly place in the interior of the board.
    /// </summary>
    [Tooltip("Array of ground tile assets to randomly place in the interior")]
    public Tile[] GroundTiles;
    
    /// <summary>
    /// Array of wall tile assets to randomly place around the perimeter of the board.
    /// </summary>
    [Tooltip("Array of wall tile assets to randomly place around the perimeter")]
    public Tile[] WallTiles;
    
    /// <summary>
    /// Reference to the PlayerController that will be spawned on this board.
    /// </summary>
    [Tooltip("Reference to the PlayerController that will be spawned on this board")]
    public PlayerController Player;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Called once before the first execution of Update after the MonoBehaviour is created.
    /// Initializes the board by finding required components, generating the tile layout, and spawning the player.
    /// </summary>
    void Start()
    {
        // Find required components
        m_Grid = GetComponentInChildren<Grid>();
        m_Tilemap = GetComponentInChildren<Tilemap>();

        // Validate required components
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

        // Initialize board data structure
        m_BoardData = new CellData[Width, Height];

        // Generate the board layout
        GenerateBoard();
        
        // Spawn the player if reference is valid
        if (Player == null)
        {
            Debug.LogError("Player reference is null! Please assign the PlayerController in the Inspector.");
            return;
        }
        // Spawn player at (1, 1)
        Player.Spawn(this, new Vector2Int(1, 1));
    }

    /// <summary>
    /// Called once per frame. Currently unused but kept for future frame-based logic.
    /// </summary>
    void Update()
    {

    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Generates the board layout by placing wall tiles around the perimeter and ground tiles in the interior.
    /// Also initializes the CellData for each cell to track passability.
    /// </summary>
    private void GenerateBoard()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tileToPlace = null;
                m_BoardData[x, y] = new CellData();
                
                // Place walls on the perimeter (edges of the board)
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    int randomIndex = Random.Range(0, WallTiles.Length);
                    tileToPlace = WallTiles[randomIndex];
                    m_BoardData[x, y].Pasable = false;
                }
                // Place ground tiles in the interior
                else
                {
                    int randomIndex = Random.Range(0, GroundTiles.Length);
                    tileToPlace = GroundTiles[randomIndex];
                    m_BoardData[x, y].Pasable = true;
                }
                
                // Set the tile in the tilemap
                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tileToPlace);
            }
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Converts a 2D cell coordinate to world space position.
    /// Uses the Grid component to handle coordinate transformation and scaling.
    /// </summary>
    /// <param name="cellIndex">The cell coordinate to convert (x, y)</param>
    /// <returns>World space position at the center of the specified cell</returns>
    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    /// <summary>
    /// Retrieves the CellData for a given cell coordinate.
    /// Performs bounds checking to ensure the coordinate is valid.
    /// </summary>
    /// <param name="cellIndex">The cell coordinate to query (x, y)</param>
    /// <returns>CellData for the specified cell, or null if coordinates are out of bounds</returns>
    public CellData GetCellData(Vector2Int cellIndex)
    {
        // Check if coordinates are within board bounds
        if (cellIndex.x < 0 || cellIndex.x >= Width || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }
        return m_BoardData[cellIndex.x, cellIndex.y];
    }
    #endregion
}
