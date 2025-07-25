using UnityEngine;
using UnityEngine.UIElements;

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

    [Tooltip("Reference to the UI Document for game UI")]
    public UIDocument UIDoc;

    public TurnManager TurnManager { get; private set; }

    private int m_FoodAmount = 100;
    private Label m_FoodLabel;

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
        TurnManager.OnTick += OnTurnHappen;

        Board.Init();
        Player.Spawn(Board, new Vector2Int(1, 1));

        m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_FoodLabel.text = $"Food : {m_FoodAmount}";
    }

    void OnTurnHappen()
    {
        ChangeFoodAmount(-1);
    }

    public void ChangeFoodAmount(int amount)
    {
        m_FoodAmount += amount;
        m_FoodLabel.text = $"Food : {m_FoodAmount}";
    }
}
