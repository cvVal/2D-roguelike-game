using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player movement on a grid-based board using arrow key input.
/// Handles input detection, movement validation, and position updates in both grid and world space.
/// Requires the Unity Input System package for keyboard input detection.
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Private Fields
    /// <summary>
    /// Reference to the BoardManager that controls the game board and provides cell data.
    /// Set during the Spawn method call from BoardManager.
    /// </summary>
    private BoardManager m_BoardManager;
    
    /// <summary>
    /// Current position of the player in grid coordinates (cell-based).
    /// Used for movement calculations and collision detection.
    /// </summary>
    private Vector2Int m_CellPosition;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Called once before the first execution of Update after the MonoBehaviour is created.
    /// </summary>
    void Start()
    {

    }

    /// <summary>
    /// Called once per frame to handle player input and movement.
    /// Detects arrow key presses and attempts to move the player to adjacent cells.
    /// Validates movement against board boundaries and cell passability.
    /// </summary>
    void Update()
    {
        // Safety check to prevent null reference errors
        if (m_BoardManager == null)
        {
            Debug.LogWarning("BoardManager is null in PlayerController. Spawn method may not have been called yet.");
            return;
        }

        // Calculate target cell based on input
        Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;
        
        // Check for arrow key input and update target position
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y += 1; // Move up
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y -= 1; // Move down
            hasMoved = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x -= 1; // Move left
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x += 1; // Move right
            hasMoved = true;
        }

        // Process movement if input was detected
        if (hasMoved)
        {
            // Validate the target cell is within bounds and passable
            BoardManager.CellData cellData = m_BoardManager.GetCellData(newCellTarget);
            if (cellData != null && cellData.Pasable)
            {
                MoveTo(newCellTarget);
            }
            // If cellData is null (out of bounds) or not passable, movement is blocked
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes the player at a specific cell position on the board.
    /// Called by the BoardManager during level initialization.
    /// Sets up the BoardManager reference and moves the player to the specified starting position.
    /// </summary>
    /// <param name="boardManager">Reference to the BoardManager that controls the game board</param>
    /// <param name="cell">Starting cell position for the player (x, y coordinates)</param>
    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_BoardManager = boardManager;
        MoveTo(cell);
    }

    /// <summary>
    /// Moves the player to a specific cell position.
    /// Updates both the grid position and world transform position.
    /// </summary>
    /// <param name="cell">Target cell position to move to (x, y coordinates)</param>
    public void MoveTo(Vector2Int cell)
    {
        m_CellPosition = cell;
        transform.position = m_BoardManager.CellToWorld(m_CellPosition);
    }
    #endregion
}
