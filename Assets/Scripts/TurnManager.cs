using UnityEngine;

/// <summary>
/// Manages the turn-based logic for the game.
/// </summary>
public class TurnManager
{
    public event System.Action OnTick;

    private int m_TurnCount;

    public TurnManager()
    {
        m_TurnCount = 1;
    }

    public void Tick()
    {
        OnTick?.Invoke();
        m_TurnCount++;
    }
}
