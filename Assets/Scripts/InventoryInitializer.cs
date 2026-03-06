using UnityEngine;

/// <summary>
/// 负责初始化库存系统的组件，将预设物品放置到库存槽位中
/// </summary>
public class InventoryInitializer : MonoBehaviour
{
    /// <summary>
    /// 初始时要放置到库存中的物品数据数组
    /// </summary>
    public ItemData[] initialItems;
    
    /// <summary>
    /// 所有库存槽位的引用数组
    /// </summary>
    private InventorySlot[] slots;

    /// <summary>
    /// Unity生命周期方法，在对象启用时执行库存初始化
    /// </summary>
    void Start()
    {
        // 查找场景中所有的库存槽位组件
        slots = FindObjectsOfType<InventorySlot>();
        int index = 0;

        // 遍历所有初始物品并尝试放置到槽位中
        for (int i = 0; i < initialItems.Length && index < slots.Length; i++)
        {
            ItemData item = initialItems[i];
            if (item == null) continue;

            // 确保物品尺寸至少为1x1
            Vector2Int size = item.size;
            if (size.x < 1) size.x = 1;
            if (size.y < 1) size.y = 1;

            // 处理宽度大于1的大型物品
            if (size.x > 1)
            {
                bool canPlace = true;
                // 检查目标位置是否有足够的连续空槽位
                for (int dx = 0; dx < size.x; dx++)
                {
                    int targetIdx = index + dx;
                    if (targetIdx >= slots.Length || slots[targetIdx].currentItem != null)
                    {
                        canPlace = false;
                        break;
                    }
                }
                if (!canPlace)
                {
                    index++;
                    i--;
                    continue;
                }
                
                // 实例化大型物品的预制体并设置属性
                GameObject obj = Instantiate(item.prefab, slots[index].transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.tag = "DraggableItem";
                obj.layer = LayerMask.NameToLayer("Item");
                
                // 将物品信息设置到所有占用的槽位中
                for (int dx = 0; dx < size.x; dx++)
                {
                    slots[index + dx].SetItem(item, obj);
                }
                index += size.x;
            }
            else
            {
                // 处理单格物品的放置
                slots[index].SetItem(item);
                if (item.prefab != null)
                {
                    GameObject obj = Instantiate(item.prefab, slots[index].transform);
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                    obj.tag = "DraggableItem";
                    obj.layer = LayerMask.NameToLayer("Item");
                    slots[index].occupyingObject = obj;
                }
                index++;
            }
        }
    }
}
