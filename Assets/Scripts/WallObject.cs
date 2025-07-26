using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    public Tile ObstacleTile;
    public Tile DamagedTile;
    public int MaxHealth = 3;

    private int m_HealthPoint;
    private Tile m_OriginalTile;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);

        m_HealthPoint = MaxHealth;
        m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
        GameManager.Instance.BoardManager.SetCellTile(cell, ObstacleTile);
    }

    public override bool PlayerWantsToEnter()
    {
        m_HealthPoint -= 1;
        
        // Show damaged state when at half health (but only once)
        if (m_HealthPoint == MaxHealth / 2)
        {
            GameManager.Instance.BoardManager.SetCellTile(m_cell, DamagedTile);
        }
        
        // Wall is destroyed
        if (m_HealthPoint <= 0)
        {
            GameManager.Instance.BoardManager.SetCellTile(m_cell, m_OriginalTile);
            Destroy(gameObject);
            return true; // Player can now enter this cell
        }

        return false; // Wall still blocks player
    }
}
