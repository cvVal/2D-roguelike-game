using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles grid-based player movement using arrow key input.
/// </summary>
public class PlayerController : MonoBehaviour
{
    private BoardManager m_BoardManager;
    private Vector2Int m_CellPosition;
    private bool m_IsGameOver;
    private Animator m_Animator;
    private bool m_IsAttacking;

    public Vector2Int CurrentCell => m_CellPosition;

    private AudioSource m_AudioSource;
    public AudioClip AttackSound;
    public AudioClip DamagedSound;

    public void Init()
    {
        m_IsGameOver = false;
        m_IsAttacking = false;
        m_Animator = GetComponent<Animator>();
    }

    void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }

            return;
        }

        // Don't accept input during attack animation
        if (m_IsAttacking) return;

        Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y -= 1;
            hasMoved = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x -= 1;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            hasMoved = false;
            GameManager.Instance.TurnManager.Tick(); // Just advance turn without moving
        }

        if (hasMoved)
        {
            BoardManager.CellData cellData = m_BoardManager.GetCellData(newCellTarget);
            if (cellData != null && cellData.Passable)
            {
                bool playerActed = false;

                if (cellData.ContainedObject == null)
                {
                    MoveTo(newCellTarget);
                    playerActed = true;
                }
                else
                {
                    // Check if this is an attackable object (wall, enemy, etc.)
                    bool isAttackable = cellData.ContainedObject is WallObject
                        || cellData.ContainedObject is EnemyObject;

                    if (isAttackable)
                    {
                        // Attack handles its own turn advancement
                        StartCoroutine(AttackSequence(cellData.ContainedObject, newCellTarget));
                    }
                    else
                    {
                        // Non-attackable objects (like food) - no animation, just interact
                        if (cellData.ContainedObject.PlayerWantsToEnter())
                        {
                            MoveTo(newCellTarget);
                            cellData.ContainedObject.PlayerEntered();
                            playerActed = true;
                        }
                    }
                }

                // Only advance turn for non-attack actions (attacks handle their own turn advancement)
                if (playerActed)
                {
                    GameManager.Instance.TurnManager.Tick();
                }
            }
        }
    }

    /// <summary>
    /// Initializes player at specified board position.
    /// </summary>
    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_BoardManager = boardManager;
        MoveTo(cell);
    }

    /// <summary>
    /// Moves player to target cell position.
    /// </summary>
    public void MoveTo(Vector2Int cell)
    {
        m_CellPosition = cell;
        transform.position = m_BoardManager.CellToWorld(m_CellPosition);
    }

    public void GameOver()
    {
        m_IsGameOver = true;
    }

    public void Damaged()
    {
        m_Animator.SetTrigger("Damaged");
        m_AudioSource.PlayOneShot(DamagedSound);
    }

    /// <summary>
    /// Handles attack sequence with proper timing to prevent visual glitches.
    /// </summary>
    private IEnumerator AttackSequence(CellObject target, Vector2Int targetCell)
    {
        m_IsAttacking = true;

        if (m_Animator != null)
        {
            m_Animator.SetTrigger("Attack");
        }
        m_AudioSource.PlayOneShot(AttackSound);

        // Wait for a brief moment to let animation start
        yield return new WaitForSeconds(0.3f);

        if (target.PlayerWantsToEnter())
        {
            // Target was destroyed/allows entry, player can move
            MoveTo(targetCell);
            target.PlayerEntered();
        }

        // Wait for attack animation to finish
        yield return new WaitForSeconds(0.2f);

        m_IsAttacking = false;

        // Advance turn after attack is completely finished
        GameManager.Instance.TurnManager.Tick();
    }
}
