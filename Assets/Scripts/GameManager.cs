using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Main game controller that initializes and coordinates core game systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("Reference to the BoardManager that controls the game board")]
    public BoardManager BoardManager;

    [Tooltip("Reference to the PlayerController that will be spawned")]
    public PlayerController Player;

    [Tooltip("Reference to the UI Document for game UI")]
    public UIDocument UIDoc;

    public TurnManager TurnManager { get; private set; }

    private int m_FoodAmount = 20;
    private int m_CurrentLevel = 1;
    private Label m_FoodLabel;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;

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

        m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_FoodLabel.text = $"Food : {m_FoodAmount}";

        m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = UIDoc.rootVisualElement.Q<Label>("GameOverMessage");

        StartNewGame();
    }

    void OnTurnHappen()
    {
        ChangeFoodAmount(-1);
    }

    public void ChangeFoodAmount(int amount)
    {
        m_FoodAmount += amount;
        m_FoodLabel.text = $"Food : {m_FoodAmount}";

        if (m_FoodAmount <= 0)
        {
            Player.GameOver();
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = $"Game Over! \n\nYou ran out of food " +
                $"\n\nYou survived {m_CurrentLevel} days \n\nPress Enter to restart";
        }
    }

    public void NewLevel()
    {
        m_CurrentLevel++;

        BoardManager.ClearAllCellContents();
        BoardManager.Init(m_CurrentLevel);
        Player.Spawn(BoardManager, new Vector2Int(1, 1));
    }

    public void StartNewGame()
    {
        m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_FoodAmount = 20;
        m_FoodLabel.text = $"Food : {m_FoodAmount}";

        m_CurrentLevel = 1;

        BoardManager.ClearAllCellContents();
        BoardManager.Init(m_CurrentLevel);

        Player.Init();
        Player.Spawn(BoardManager, new Vector2Int(1, 1));
    }
}
