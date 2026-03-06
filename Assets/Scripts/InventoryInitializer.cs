using UnityEngine;

public class InventoryInitializer : MonoBehaviour
{
    public ItemData[] initialItems;
    private InventorySlot[] slots;

    void Start()
    {
        slots = FindObjectsOfType<InventorySlot>();
        int index = 0;

        for (int i = 0; i < initialItems.Length && index < slots.Length; i++)
        {
            ItemData item = initialItems[i];
            if (item == null) continue;

            Vector2Int size = item.size;
            if (size.x < 1) size.x = 1;
            if (size.y < 1) size.y = 1;

            if (size.x > 1)
            {
                bool canPlace = true;
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
                GameObject obj = Instantiate(item.prefab, slots[index].transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.tag = "DraggableItem";
                obj.layer = LayerMask.NameToLayer("Item");
                for (int dx = 0; dx < size.x; dx++)
                {
                    slots[index + dx].SetItem(item, obj);
                }
                index += size.x;
            }
            else
            {
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