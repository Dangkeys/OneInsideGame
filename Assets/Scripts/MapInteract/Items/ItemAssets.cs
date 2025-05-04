using UnityEngine;

public class ItemAssets : MonoBehaviour
{
    public static ItemAssets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Sprite IngredientASprite;
    public Sprite IngredientBSprite;
    public Sprite IngredientCSprite;
    public Sprite IngredientDSprite;
    public Sprite IngredientESprite;
    public Sprite ItemASprite;
    public Sprite ItemBSprite;
    public Sprite ItemCSprite;
}
