using UnityEngine;

public class Item
{
    public enum ItemType
    {
        IngredientA,
        IngredientB,
        IngredientC
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
            default:
                return null;
        }
    }
}
