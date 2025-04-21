using UnityEngine;

public class ItemWorld : MonoBehaviour
{
    public Item.ItemType ItemType;

    public Item GetItem()
    {
        return new Item { IType = ItemType };
    }
}