using UnityEngine;

[CreateAssetMenu(fileName = "NewRule", menuName = "Inventory/CraftingRule")]
public class CraftingRule : ScriptableObject
{
    public ItemData baseItem;
    public ItemData secondaryItem;
    public ItemData resultItem;
}