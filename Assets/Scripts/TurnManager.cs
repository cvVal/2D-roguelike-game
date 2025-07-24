using UnityEngine;

/// <summary>
/// Manages the turn-based logic for the game.
/// </summary>
public class TurnManager
{
    private int m_TurnCount;

    public TurnManager()
    {
        m_TurnCount = 1;
    }

    public void Tick()
    {
        m_TurnCount++;
        Debug.Log($"Current turn: {m_TurnCount}.");
    }
}
