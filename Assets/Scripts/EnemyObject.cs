using System;
using UnityEngine;

public class EnemyObject : CellObject
{
    public int Health = 3;
    private int m_CurrentHealth;
    private Animator m_Animator;

    void Awake()
    {
        GameManager.Instance.TurnManager.OnTick += TurnHappened;
        m_Animator = GetComponent<Animator>();
    }

    private void OnDestroy()
    {
        GameManager.Instance.TurnManager.OnTick -= TurnHappened;
    }

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);
        m_CurrentHealth = Health;
    }

    public override bool PlayerWantsToEnter()
    {
        m_CurrentHealth -= 1;
        Debug.Log($"Enemy hit! Remaining health: {m_CurrentHealth}");

        if (m_CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }

        return false;
    }

    private bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null || !targetCell.Passable || targetCell.ContainedObject != null)
        {
            return false;
        }

        // Remove from current cell
        var currentCellData = board.GetCellData(m_cell);
        currentCellData.ContainedObject = null;

        // Add it to the new cell
        targetCell.ContainedObject = this;
        m_cell = coord;
        transform.position = board.CellToWorld(coord);

        return true;
    }

    private bool TryMoveInX(int xDist)
    {
        // Try to get closer to the player in X direction

        // If player is to the right
        if (xDist > 0)
        {
            return MoveTo(m_cell + Vector2Int.right);
        }

        // Player is to the left
        return MoveTo(m_cell + Vector2Int.left);
    }

    private bool TryMoveInY(int yDist)
    {
        // Try to get closer to the player in Y direction

        // If player is above
        if (yDist > 0)
        {
            return MoveTo(m_cell + Vector2Int.up);
        }

        // Player is below
        return MoveTo(m_cell + Vector2Int.down);
    }

    private void TurnHappened()
    {
        // Skip turn if enemy is dead or being destroyed
        if (m_CurrentHealth <= 0 || this == null)
            return;
            
        var playerCell = GameManager.Instance.Player.CurrentCell;

        int xDist = playerCell.x - m_cell.x;
        int yDist = playerCell.y - m_cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        if ((xDist == 0 && absYDist == 1) || (yDist == 0 && absXDist == 1))
        {
            // Player is adjacent, attack!
            m_Animator.SetTrigger("EnemyAttack");
            GameManager.Instance.ChangeFoodAmount(-3);
        }
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }
}
