using UnityEngine;

public class Item
{
    public enum ItemType
    {
        IngredientA,
        IngredientB,
        IngredientC,
        IngredientD,
        IngredientE
    }

    public ItemType IType;

    public Sprite GetSprite()
    {
        switch (IType)
        {
            case ItemType.IngredientA:
                return ItemAssets.Instance.IngredientASprite;
            case ItemType.IngredientB:
                return ItemAssets.Instance.IngredientBSprite;
            case ItemType.IngredientC:
                return ItemAssets.Instance.IngredientCSprite;
            case ItemType.IngredientD:
                return ItemAssets.Instance.IngredientDSprite;
            case ItemType.IngredientE:
                return ItemAssets.Instance.IngredientESprite;
            default:
                Debug.LogError("Item type not found: " + IType);
                return null;
        }
    }
}
