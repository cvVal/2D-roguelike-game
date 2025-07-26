using System.Collections.Generic;
using Unity.VisualScripting;
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

    [Tooltip("Prefab for wall objects that can be placed on the board")]
    public WallObject[] WallPrefabs;

    [Tooltip("Prefab for exit cell that allows players to finish the level")]
    public ExitCellObject ExitPrefab;

    [Header("Food Settings")]

    [Tooltip("Amount of food items to spawn on the board")]
    public int foodAmount = 5;

    [Tooltip("Prefab for food items that can be placed on the board")]
    public FoodObject[] FoodPrefabs;

    private CellData[,] m_BoardData;
    private Grid m_Grid;
    private Tilemap m_Tilemap;
    private List<Vector2Int> m_EmptyCellsList;

    /// <summary>
    /// Initializes board components and generates the level layout.
    /// </summary>
    public void Init()
    {
        InitializeComponents();
        GenerateBoard();
        GenerateExit();
        GenerateWall();
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

        m_EmptyCellsList = new List<Vector2Int>();
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
                    m_EmptyCellsList.Add(new Vector2Int(x, y)); // Track empty cells for food placement
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tileToPlace);
            }
        }
        m_EmptyCellsList.Remove(new Vector2Int(1, 1)); // It's reserved for the player spawn
    }

    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    private void GenerateFood()
    {
        int foodCount = Random.Range(1, foodAmount + 1);
        for (int i = 0; i < foodCount; i++)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex); // Remove to avoid duplicates

            FoodObject foodPrefab = FoodPrefabs[Random.Range(0, FoodPrefabs.Length)];
            FoodObject newFood = Instantiate(foodPrefab);
            AddObject(newFood, coord); // Initialize food with cell coordinates
        }
    }

    private void GenerateWall()
    {
        int wallCount = Random.Range(6, 10);
        for (int i = 0; i < wallCount; i++)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            WallObject newWall = Instantiate(WallPrefabs[Random.Range(0, WallPrefabs.Length)]);
            AddObject(newWall, coord); // Initialize wall with cell coordinates
        }
    }

    private void GenerateExit()
    {
        Vector2Int exitCoord = new(Width - 2, Height - 2);
        AddObject(Instantiate(ExitPrefab), exitCoord);
        m_EmptyCellsList.Remove(exitCoord);
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public void ClearAllCellContents()
    {
        if (m_BoardData == null) return;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var cellData = m_BoardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    //CAREFUL! Destroy the GameObject NOT just cellData.ContainedObject
                    //Otherwise what you are destroying is the JUST CellObject COMPONENT
                    //and not the whole gameobject with sprite
                    Destroy(cellData.ContainedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }
}
