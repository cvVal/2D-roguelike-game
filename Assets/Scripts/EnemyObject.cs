using System;
using UnityEngine;

/// <summary>
/// Enemy AI that uses Manhattan distance pathfinding to chase the player.
/// Uses greedy algorithm: always moves toward player in the direction with the largest gap.
/// Movement priority: larger distance gets priority, with fallback to secondary direction if blocked.
/// </summary>
public class EnemyObject : CellObject
{
    public int Health = 3;
    public int DamageAmount = -1;
    private int m_CurrentHealth;
    private Animator m_Animator;
    private AudioSource m_AudioSource;
    public AudioClip AttackSound;

    void Awake()
    {
        GameManager.Instance.TurnManager.OnTick += TurnHappened;
        m_Animator = GetComponent<Animator>();
        m_AudioSource = GetComponent<AudioSource>();
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

    /// <summary>
    /// Core AI behavior triggered each turn. Uses Manhattan distance to chase player.
    /// Movement Algorithm:
    /// 1. Calculate X and Y distances to player
    /// 2. Check if adjacent (attack range)
    /// 3. Move toward player in direction with largest distance gap
    /// 4. If primary direction blocked, try secondary direction
    /// </summary>
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
            m_AudioSource.PlayOneShot(AttackSound);

            GameManager.Instance.Player.Damaged();
            GameManager.Instance.ChangeFoodAmount(DamageAmount);
        }
        else
        {
            // Manhattan pathfinding: move in direction with largest distance gap
            if (absXDist > absYDist)
            {
                // Horizontal gap is larger, then prioritize X movement
                if (!TryMoveInX(xDist))
                {
                    TryMoveInY(yDist);  // Fallback to Y if X blocked
                }
            }
            else
            {
                // Vertical gap is larger (or equal), then prioritize Y movement
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);  // Fallback to X if Y blocked
                }
            }
        }
    }
}
