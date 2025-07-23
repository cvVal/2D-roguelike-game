using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Pasable;
    }

    private CellData[,] m_BoardData;
    private Grid m_Grid;
    private Tilemap m_Tilemap;

    public int Width;
    public int Height;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;
    public PlayerController Player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Grid = GetComponent<Grid>();
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

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tileToPlace = null;
                m_BoardData[x, y] = new CellData();
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    int randomIndex = Random.Range(0, WallTiles.Length);
                    tileToPlace = WallTiles[randomIndex];
                    m_BoardData[x, y].Pasable = false;
                }
                else
                {
                    int randomIndex = Random.Range(0, GroundTiles.Length);
                    tileToPlace = GroundTiles[randomIndex];
                    m_BoardData[x, y].Pasable = true;
                }
                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tileToPlace);
            }
        }
        Player.Spawn(this, new Vector2Int(1, 1)); // Spawn player at (1, 1)
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }
}
