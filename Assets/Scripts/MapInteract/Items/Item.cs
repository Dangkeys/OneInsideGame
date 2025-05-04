using UnityEngine;

public class Item
{
    public enum ItemType
    {
        IngredientA,
        IngredientB,
        IngredientC,
        IngredientD,
        IngredientE,
        ItemA,
        ItemB,
        ItemC
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
            case ItemType.ItemA:
                return ItemAssets.Instance.ItemASprite;
            case ItemType.ItemB:
                return ItemAssets.Instance.ItemBSprite;
            case ItemType.ItemC:
                return ItemAssets.Instance.ItemCSprite;
            default:
                return null;
        }
    }
}
