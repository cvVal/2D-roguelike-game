using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages board generation and provides grid-world coordinate conversion utilities.
/// 
/// Progressive Difficulty System:
/// • Levels 1-2: 8x8 boards, generous food, minimal walls, no enemies (learning phase)
/// • Levels 3-5: 10x10 boards, balanced resources, moderate obstacles (skill building)  
/// • Levels 6-8: 12x12 boards, reduced food, tactical walls, multiple enemies (challenge)
/// • Levels 9-12: 14x14 boards, scarce food, dense walls, many enemies (mastery)
/// • Levels 13+: 16x16 boards, maintained difficulty with scaling density (endgame)
/// </summary>
public class BoardManager : MonoBehaviour
{
    [System.Serializable]
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    [System.Serializable]
    public class LevelConfig
    {
        [Header("Board Settings")]
        public int BoardWidth = 8;
        public int BoardHeight = 8;

        [Header("Enemy Settings")]
        public int MinEnemies = 0;
        public int MaxEnemies = 0;
        public bool IsEnemyPositionFixed = false;

        [Header("Resource Scaling")]
        public int FoodDivisor = 8;     // boardArea / foodDivisor = food amount
        public int MinFood = 4;
        public int WallDivisor = 20;    // boardArea / wallDivisor = wall amount  
        public int MinWalls = 2;

        [Header("Traps and Hazards")]
        public int MinTraps = 0;
        public int MaxTraps = 0;

        [Header("Camera Settings")]
        public float CameraLensSize = 3f; // Cinemachine lens orthographic size
    }

    [Header("Level Progression")]
    [SerializeField]
    private LevelConfig[] levelConfigs = new LevelConfig[]
    {
        new LevelConfig {
            BoardWidth = 8, BoardHeight = 8,
            MinEnemies = 0, MaxEnemies = 0,
            FoodDivisor = 8, MinFood = 4,
            WallDivisor = 20, MinWalls = 2
        },

        new LevelConfig {
            BoardWidth = 10, BoardHeight = 10,
            MinEnemies = 1, MaxEnemies = 1, IsEnemyPositionFixed = true,
            FoodDivisor = 12, MinFood = 3,
            WallDivisor = 15, MinWalls = 4
        },

        new LevelConfig {
            BoardWidth = 12, BoardHeight = 12,
            MinEnemies = 2, MaxEnemies = 4,
            FoodDivisor = 16, MinFood = 2,
            WallDivisor = 12, MinWalls = 6,
            MinTraps = 2, MaxTraps = 4,
            CameraLensSize = 3.5f
        },

        new LevelConfig {
            BoardWidth = 14, BoardHeight = 14,
            MinEnemies = 3, MaxEnemies = 6,
            FoodDivisor = 20, MinFood = 3,
            WallDivisor = 10, MinWalls = 8,
            MinTraps = 3, MaxTraps = 5,
            CameraLensSize = 4.0f
        },

        new LevelConfig {
            BoardWidth = 16, BoardHeight = 16,
            MinEnemies = 5, MaxEnemies = 9,
            FoodDivisor = 20, MinFood = 3,
            WallDivisor = 10, MinWalls = 8,
            MinTraps = 5, MaxTraps = 8,
            CameraLensSize = 4.5f
        }
    };

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

    [Tooltip("Prefab for enemy objects that can be placed on the board")]
    public EnemyObject[] EnemyPrefabs;

    [Header("Food Settings")]

    [Tooltip("Initial amount of food items to spawn")]
    public int InitialFoodAmount = 3;

    [Tooltip("Prefab for food items that can be placed on the board")]
    public FoodObject[] FoodPrefabs;

    [Header("Trap Settings")]
    [Tooltip("Prefab for trap objects that can be placed on the board")]
    public TrapObject[] TrapPrefabs;

    [Header("Camera Settings")]

    [Tooltip("Cinemachine Camera Controller that handles lens size updates")]
    public CinemachineCameraController CameraController;

    private CellData[,] m_BoardData;
    private Grid m_Grid;
    private Tilemap m_Tilemap;
    private List<Vector2Int> m_EmptyCellsList;
    private LevelConfig m_CurrentLevelConfig;

    /// <summary>
    /// Gets the appropriate level configuration based on current level.
    /// </summary>
    private LevelConfig GetLevelConfig(int currentLevel)
    {
        if (currentLevel <= 2) return levelConfigs[0]; // Levels 1-2: 8x8 boards
        if (currentLevel <= 5) return levelConfigs[1]; // Levels 3-5: 10x10 boards
        if (currentLevel <= 8) return levelConfigs[2]; // Levels 6-8: 12x12 boards
        if (currentLevel <= 12) return levelConfigs[3]; // Levels 9-12: 14x14 boards
        return levelConfigs[4]; // Levels 13+: 16x16 boards
    }

    /// <summary>
    /// Initializes board components and generates the level layout.
    /// </summary>
    public void Init(int currentLevel)
    {
        InitializeComponents(currentLevel);
        UpdateCameraLens();
        GenerateBoard();
        GenerateExit();
        GenerateEnemy(currentLevel);
        GenerateWall(currentLevel);
        GenerateFood(currentLevel);
        GenerateTrap(currentLevel);
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

    private void InitializeComponents(int currentLevel)
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

        // Get configuration for current level
        m_CurrentLevelConfig = GetLevelConfig(currentLevel);

        Width = m_CurrentLevelConfig.BoardWidth;
        Height = m_CurrentLevelConfig.BoardHeight;

        m_BoardData = new CellData[Width, Height];
    }

    private void UpdateCameraLens()
    {
        CameraController.UpdateLensSize(m_CurrentLevelConfig.CameraLensSize);
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

    /// <summary>
    /// Generates food items with progressive scaling based on level configuration.
    /// Uses data-driven approach for clean, maintainable difficulty progression.
    /// </summary>
    private void GenerateFood(int currentLevel)
    {
        int boardArea = (Width - 2) * (Height - 2); // Interior area only
        int scaledFoodAmount = Mathf.Max(m_CurrentLevelConfig.MinFood,
                                        boardArea / m_CurrentLevelConfig.FoodDivisor);

        int foodCount = Random.Range(scaledFoodAmount, scaledFoodAmount + 2);

        for (int i = 0; i < foodCount; i++)
        {
            if (m_EmptyCellsList.Count == 0) break; // Safety check

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex); // Remove to avoid duplicates

            FoodObject foodPrefab = FoodPrefabs[Random.Range(0, FoodPrefabs.Length)];
            FoodObject newFood = Instantiate(foodPrefab);
            AddObject(newFood, coord); // Initialize food with cell coordinates
        }
    }

    /// <summary>
    /// Generates traps with progressive density using level configuration.
    /// </summary>
    private void GenerateTrap(int currentLevel)
    {
        int trapCount = Random.Range(m_CurrentLevelConfig.MinTraps, m_CurrentLevelConfig.MaxTraps + 1);

        if (trapCount == 0) return; // No traps for early levels

        for (int i = 0; i < trapCount; i++)
        {
            if (m_EmptyCellsList.Count == 0) break; // Safety check

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            TrapObject newTrap = Instantiate(TrapPrefabs[Random.Range(0, TrapPrefabs.Length)]);
            AddObject(newTrap, coord); // Initialize trap with cell coordinates
        }
    }

    /// <summary>
    /// Generates destructible walls with progressive density using level configuration.
    /// </summary>
    private void GenerateWall(int currentLevel)
    {
        int boardArea = (Width - 2) * (Height - 2); // Interior area only
        int baseWallCount = Mathf.Max(m_CurrentLevelConfig.MinWalls,
                                     boardArea / m_CurrentLevelConfig.WallDivisor);

        int wallCount = Random.Range(baseWallCount, baseWallCount + 3);

        for (int i = 0; i < wallCount; i++)
        {
            if (m_EmptyCellsList.Count == 0) break; // Safety check

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

    /// <summary>
    /// Generates enemies with progressive scaling using level configuration.
    /// </summary>
    private void GenerateEnemy(int currentLevel)
    {
        int enemyCount = Random.Range(m_CurrentLevelConfig.MinEnemies,
                                     m_CurrentLevelConfig.MaxEnemies + 1);

        if (enemyCount == 0) return; // No enemies for early levels

        // Use fixed position for early single-enemy levels
        if (m_CurrentLevelConfig.IsEnemyPositionFixed && enemyCount == 1)
        {
            Vector2Int enemyCoord = new(Width - 3, Height - 3);
            SpawnEnemyAt(enemyCoord);
        }
        else
        {
            // Multiple enemies at random positions
            SpawnRandomEnemies(enemyCount);
        }
    }

    private void SpawnEnemyAt(Vector2Int coord)
    {
        AddObject(Instantiate(EnemyPrefabs[0]), coord);
        m_EmptyCellsList.Remove(coord);
    }

    private void SpawnRandomEnemies(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (m_EmptyCellsList.Count == 0) break; // Safety check

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int enemyCoord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);
            EnemyObject newEnemy = Instantiate(EnemyPrefabs[Random.Range(0, EnemyPrefabs.Length)]);
            AddObject(newEnemy, enemyCoord);
        }
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
