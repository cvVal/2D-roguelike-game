using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool pasable;
    }

    private CellData[,] boardData;
    private Tilemap tilemap;

    public int width;
    public int height;
    public Tile[] groundTiles;
    public Tile[] wallTiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tilemap = GetComponentInChildren<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogError("Tilemap component not found in children.");
            return;
        }

        boardData = new CellData[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tileToPlace = null;
                boardData[x, y] = new CellData();
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    int randomIndex = Random.Range(0, wallTiles.Length);
                    tileToPlace = wallTiles[randomIndex];
                    boardData[x, y].pasable = false;
                }
                else
                {
                    int randomIndex = Random.Range(0, groundTiles.Length);
                    tileToPlace = groundTiles[randomIndex];
                    boardData[x, y].pasable = true;
                }
                tilemap.SetTile(new Vector3Int(x, y, 0), tileToPlace);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
