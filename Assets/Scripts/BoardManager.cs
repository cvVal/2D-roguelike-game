using System.Collections.Generic;
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
        public CellObject ContainedObject;
    }

    [Header("Board Settings")]

    [Tooltip("Width of the board in grid cells")]
    public int Width;

    [Tooltip("Height of the board in grid cells")]
    public int Height;

    [Tooltip("Ground tiles for interior placement")]
    public Tile[] GroundTiles;

    [Tooltip("Wall tiles for perimeter placement")]
    public Tile[] WallTiles;

    [Header("Food Settings")]

    [Tooltip("Amount of food items to spawn on the board")]
    public int foodAmount = 5;

    [Tooltip("Prefab for food items that can be placed on the board")]
    public FoodObject[] FoodPrefabs;

    private CellData[,] m_BoardData;
    private Grid m_Grid;
    private Tilemap m_Tilemap;
    private List<Vector2Int> m_EmptyCellList;

    /// <summary>
    /// Initializes board components and generates the level layout.
    /// </summary>
    public void Init()
    {
        InitializeComponents();
        GenerateBoard();
        GenerateFood();
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
    /// Used by PlayerController for movement validation.
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

        m_EmptyCellList = new List<Vector2Int>();
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
                    m_EmptyCellList.Add(new Vector2Int(x, y)); // Track empty cells for food placement
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tileToPlace);
            }
        }
        m_EmptyCellList.Remove(new Vector2Int(1, 1)); // It's reserved for the player spawn
    }

    private void GenerateFood()
    {
        int foodCount = Random.Range(1, foodAmount + 1);
        for (int i = 0; i < foodCount; i++)
        {
            int randomIndex = Random.Range(0, m_EmptyCellList.Count);
            Vector2Int coord = m_EmptyCellList[randomIndex];

            m_EmptyCellList.RemoveAt(randomIndex); // Remove to avoid duplicates
            CellData cellData = m_BoardData[coord.x, coord.y];
            if (cellData.Passable && cellData.ContainedObject == null)
            {
                FoodObject foodPrefab = FoodPrefabs[Random.Range(0, FoodPrefabs.Length)];
                FoodObject newFood = Instantiate(foodPrefab);
                newFood.transform.position = CellToWorld(coord);
                cellData.ContainedObject = newFood;
            }
        }
    }
}
