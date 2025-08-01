using UnityEngine;

public class TrapObject : CellObject
{
    public AudioSource AudioSource;
    public AudioClip TrapSound;
    public int DamageAmount = 5;

    public override void PlayerEntered()
    {
        if (AudioSource != null && TrapSound != null)
        {
            AudioSource.PlayOneShot(TrapSound);
        }
        Invoke(nameof(TriggerTrap), 0.1f);
    }

    private void TriggerTrap()
    {
        Destroy(gameObject);
        Debug.Log($"Player triggered a trap! Damage dealt: {DamageAmount}");

        GameManager.Instance.Player.Damaged();
        GameManager.Instance.ChangeFoodAmount(-DamageAmount);
    }
}
