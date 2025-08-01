using UnityEngine;

public class FoodObject : CellObject
{
    public AudioSource AudioSource;
    public AudioClip CollectSound;
    public int NutritionValue = 5;

    public override void PlayerEntered()
    {
        if (AudioSource != null && CollectSound != null)
        {
            AudioSource.PlayOneShot(CollectSound);
        }
        Invoke(nameof(CollectFood), 0.1f);
    }

    private void CollectFood()
    {
        Destroy(gameObject);
        Debug.Log($"Food collected! Amount granted: {NutritionValue}");
        GameManager.Instance.ChangeFoodAmount(NutritionValue);
    }
}
