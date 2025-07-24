using UnityEngine;

/// <summary>
/// Main game controller that initializes and coordinates core game systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("Reference to the BoardManager that controls the game board")]
    public BoardManager Board;

    [Tooltip("Reference to the PlayerController that will be spawned")]
    public PlayerController Player;

    public TurnManager TurnManager { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        TurnManager = new TurnManager();

        Board.Init();
        Player.Spawn(Board, new Vector2Int(1, 1));
    }
}
