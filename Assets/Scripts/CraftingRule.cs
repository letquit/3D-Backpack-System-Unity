using UnityEngine;

/// <summary>
/// 制作规则脚本对象，定义物品制作的配方规则
/// </summary>
[CreateAssetMenu(fileName = "NewRule", menuName = "Inventory/CraftingRule")]
public class CraftingRule : ScriptableObject
{
    /// <summary>
    /// 基础物品，制作配方中的主要材料
    /// </summary>
    public ItemData baseItem;
    
    /// <summary>
    /// 次要物品，制作配方中的辅助材料
    /// </summary>
    public ItemData secondaryItem;
    
    /// <summary>
    /// 结果物品，制作完成后获得的产物
    /// </summary>
    public ItemData resultItem;
}
