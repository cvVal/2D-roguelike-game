using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private BoardManager m_BoardManager;
    private Vector2Int m_CellPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_BoardManager = boardManager;
        m_CellPosition = cell;

        // Set the player's position based on the cell position
        transform.position = m_BoardManager.CellToWorld(m_CellPosition);
    }
}
