using UnityEngine;

/// <summary>
/// 物品数据脚本对象类，用于存储游戏物品的基本信息
/// </summary>
/// <remarks>
/// 该类继承自ScriptableObject，可以通过Unity编辑器创建物品数据资产文件
/// </remarks>
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    /// <summary>
    /// 物品名称
    /// </summary>
    public string itemName;
    
    /// <summary>
    /// 物品图标精灵
    /// </summary>
    public Sprite icon;
    
    /// <summary>
    /// 物品预制体对象
    /// </summary>
    public GameObject prefab;
    
    /// <summary>
    /// 物品唯一标识符
    /// </summary>
    public string itemId;
    
    /// <summary>
    /// 物品在背包中的占用格子大小，默认为1x1
    /// </summary>
    public Vector2Int size = new Vector2Int(1, 1);
}
