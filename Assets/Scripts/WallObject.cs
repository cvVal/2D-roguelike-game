using UnityEngine;
using UnityEngine.Tilemaps;

public class WallObject : CellObject
{
    public Tile ObstacleTile;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);
        GameManager.Instance.Board.SetCellTile(cell, ObstacleTile);
    }

    public override bool PlayerWantsToEnter()
    {
        return false;
    }
}
