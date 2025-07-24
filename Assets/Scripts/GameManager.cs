using UnityEngine;

/// <summary>
/// Main game controller that initializes and coordinates core game systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Tooltip("Reference to the BoardManager that controls the game board")]
    public BoardManager Board;

    [Tooltip("Reference to the PlayerController that will be spawned")]
    public PlayerController Player;

    private TurnManager m_TurnManager;

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {

    }

    private void InitializeGame()
    {
        m_TurnManager = new TurnManager();

        if (Board == null)
        {
            Debug.LogError("Board reference is null! Please assign the BoardManager in the Inspector.");
            return;
        }
        
        Board.Init();

        if (Player == null)
        {
            Debug.LogError("Player reference is null! Please assign the PlayerController in the Inspector.");
            return;
        }
        
        Player.Spawn(Board, new Vector2Int(1, 1));
    }
}
