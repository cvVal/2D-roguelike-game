using UnityEngine;

public class FoodObject : CellObject
{
    public int NutritionValue = 5;

    public override void PlayerEntered()
    {
        Destroy(gameObject);
        Debug.Log($"Food collected! Amount granted: {NutritionValue}");
        GameManager.Instance.ChangeFoodAmount(NutritionValue);
    }
}
