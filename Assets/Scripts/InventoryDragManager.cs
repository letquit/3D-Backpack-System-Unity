using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 管理库存拖拽操作的单例管理器
/// 处理物品拖拽、放置、交换和合成等操作
/// </summary>
public class InventoryDragManager : MonoBehaviour
{
    /// <summary>
    /// 获取当前实例
    /// </summary>
    public static InventoryDragManager Instance { get; private set; }

    [Header("Settings")]
    public Camera mainCamera;
    public LayerMask itemLayer;
    public LayerMask slotLayer;
    public CraftingRule[] craftingRules;

    private GameObject _draggedObject;
    private InventorySlot _draggedFromSlot;
    private InventorySlot _hoveredSlot;
    private ItemData _draggedItemData;
    private bool _isDragging = false;
    private Vector3 _dragOffset;

    private InventorySlot[] slots;
    
    /// <summary>
    /// 初始化单例实例并设置相机和槽位数组
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        if (mainCamera == null) mainCamera = Camera.main;
        
        // 查找所有库存槽位并按名称排序
        slots = FindObjectsOfType<InventorySlot>();
        System.Array.Sort(slots, (a, b) => string.Compare(a.name, b.name));
    }

    /// <summary>
    /// 更新拖拽状态，检查拾取或处理拖拽操作
    /// </summary>
    private void Update()
    {
        if (!_isDragging)
        {
            CheckPickup();
        }
        else
        {
            HandleDragging();
            HandleRotation();
            CheckSlotWithRaycast();
            CheckDrop();
        }
    }

    /// <summary>
    /// 使用射线检测检查鼠标悬停的槽位
    /// </summary>
    private void CheckSlotWithRaycast()
    {
        if (_draggedObject == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, slotLayer))
        {
            InventorySlot newSlot = hit.collider.GetComponent<InventorySlot>();
            if (newSlot != null)
            {
                if (_hoveredSlot != null && _hoveredSlot != newSlot)
                {
                    _hoveredSlot.SetHover(false);
                }
                _hoveredSlot = newSlot;
                _hoveredSlot.SetHover(true);
            }
        }
        else
        {
            if (_hoveredSlot != null)
            {
                _hoveredSlot.SetHover(false);
                _hoveredSlot = null;
            }
        }
    }

    /// <summary>
    /// 检查鼠标点击以开始拖拽操作
    /// </summary>
    private void CheckPickup()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, itemLayer))
            {
                if (IsDraggableItem(hit.collider))
                {
                    GameObject rootObj = GetRootDraggableObject(hit.collider.gameObject);
                    StartDrag(rootObj);
                }
            }
        }
    }

    /// <summary>
    /// 检查碰撞体是否为可拖拽物品
    /// </summary>
    /// <param name="col">要检查的碰撞体</param>
    /// <returns>如果为可拖拽物品则返回true</returns>
    private bool IsDraggableItem(Collider col)
    {
        return col.CompareTag("DraggableItem") ||
               (col.transform.parent != null && col.transform.parent.CompareTag("DraggableItem"));
    }

    /// <summary>
    /// 获取可拖拽对象的根游戏对象
    /// </summary>
    /// <param name="obj">起始游戏对象</param>
    /// <returns>根可拖拽对象</returns>
    private GameObject GetRootDraggableObject(GameObject obj)
    {
        Transform t = obj.transform;
        while (t.parent != null)
        {
            if (t.parent.CompareTag("DraggableItem"))
                return t.parent.gameObject;
            t = t.parent;
        }
        return obj;
    }

    /// <summary>
    /// 开始拖拽指定的游戏对象
    /// </summary>
    /// <param name="obj">要拖拽的对象</param>
    private void StartDrag(GameObject obj)
    {
        _isDragging = true;
        _draggedObject = obj;

        InventorySlot slot = obj.transform.parent?.GetComponent<InventorySlot>();
        if (slot == null && obj.transform.parent != null)
            slot = obj.transform.parent.GetComponentInParent<InventorySlot>();

        if (slot != null)
        {
            _draggedFromSlot = slot;
            _draggedItemData = slot.currentItem;
            _draggedObject.transform.SetParent(null);
        }
        else
        {
            _draggedFromSlot = null;
            _draggedItemData = obj.GetComponent<ItemData>();
            if (_draggedItemData == null) _draggedItemData = obj.GetComponentInChildren<ItemData>();
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(mainCamera.transform.forward, _draggedObject.transform.position);
        if (plane.Raycast(ray, out float distance))
        {
            _dragOffset = _draggedObject.transform.position - ray.GetPoint(distance);
        }
    }

    /// <summary>
    /// 处理拖拽过程中的对象位置更新
    /// </summary>
    private void HandleDragging()
    {
        if (_draggedObject == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(mainCamera.transform.forward, mainCamera.transform.position + mainCamera.transform.forward * 5f);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 targetPos = ray.GetPoint(distance) + _dragOffset;
            _draggedObject.transform.position = targetPos;
        }
    }

    /// <summary>
    /// 处理拖拽对象的旋转操作
    /// </summary>
    private void HandleRotation()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame && _draggedObject != null)
        {
            _draggedObject.transform.Rotate(0, 90, 0, Space.World);
        }
    }

    /// <summary>
    /// 检查鼠标释放以执行放置操作
    /// </summary>
    private void CheckDrop()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            PerformDrop();
        }
    }

    /// <summary>
    /// 执行放置操作，处理物品移动、交换或返回原位
    /// </summary>
    private void PerformDrop()
    {
        _isDragging = false;
        if (_hoveredSlot != null)
            _hoveredSlot.SetHover(false);

        bool success = false;

        if (_hoveredSlot != null && _hoveredSlot != _draggedFromSlot)
        {
            if (_hoveredSlot.currentItem == null)
            {
                MoveItemToSlot(_hoveredSlot);
                success = true;
            }
            else
            {
                if (TryCrafting(_hoveredSlot))
                {
                    success = true;
                }
                else
                {
                    SwapItems(_hoveredSlot);
                    success = true;
                }
            }
        }
        else
        {
            if (_draggedFromSlot != null)
                ReturnToSlot(_draggedFromSlot);
            else if (_draggedObject != null)
                Destroy(_draggedObject);
        }

        _draggedObject = null;
        _draggedFromSlot = null;
        _hoveredSlot = null;
        _draggedItemData = null;
    }

    /// <summary>
    /// 将拖拽的物品移动到目标槽位
    /// </summary>
    /// <param name="targetSlot">目标槽位</param>
    private void MoveItemToSlot(InventorySlot targetSlot)
    {
        ItemData item = _draggedItemData;
        Vector2Int size = item.size;
        int startIdx = GetSlotIndex(targetSlot);

        if (IsAreaAvailable(startIdx, size))
        {
            for (int dx = 0; dx < size.x; dx++)
            {
                InventorySlot slot = GetSlotByIndex(startIdx + dx);
                slot.SetItem(item, _draggedObject);
                if(dx == 0)
                {
                    _draggedObject.transform.SetParent(slot.transform);
                    _draggedObject.transform.localPosition = Vector3.zero;
                    _draggedObject.transform.localRotation = Quaternion.identity;
                }
            }
        }
        else
        {
            ReturnToSlot(_draggedFromSlot);
        }
    }
    
    /// <summary>
    /// 检查指定区域是否可用（无其他物品占用）
    /// </summary>
    /// <param name="startIdx">起始索引</param>
    /// <param name="size">需要的空间大小</param>
    /// <returns>如果区域可用则返回true</returns>
    private bool IsAreaAvailable(int startIdx, Vector2Int size)
    {
        for(int dx=0; dx<size.x; dx++)
        {
            InventorySlot slot = GetSlotByIndex(startIdx + dx);
            if(slot == null || slot.currentItem != null)
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// 获取指定槽位在数组中的索引
    /// </summary>
    /// <param name="slot">要查找的槽位</param>
    /// <returns>槽位索引，未找到返回-1</returns>
    private int GetSlotIndex(InventorySlot slot)
    {
        for(int i=0; i<slots.Length; i++)
            if(slots[i]==slot) return i;
        return -1;
    }

    /// <summary>
    /// 根据索引获取槽位
    /// </summary>
    /// <param name="idx">槽位索引</param>
    /// <returns>对应的槽位，无效索引返回null</returns>
    private InventorySlot GetSlotByIndex(int idx)
    {
        if(slots==null || idx<0 || idx>=slots.Length) return null;
        return slots[idx];
    }

    /// <summary>
    /// 将物品返回到原始槽位
    /// </summary>
    /// <param name="originalSlot">原始槽位</param>
    private void ReturnToSlot(InventorySlot originalSlot)
    {
        SnapToSlot(_draggedObject, originalSlot);
    }

    /// <summary>
    /// 将对象吸附到指定槽位
    /// </summary>
    /// <param name="obj">要吸附的对象</param>
    /// <param name="slot">目标槽位</param>
    private void SnapToSlot(GameObject obj, InventorySlot slot)
    {
        obj.transform.SetParent(slot.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 在两个槽位之间交换物品
    /// </summary>
    /// <param name="targetSlot">目标槽位</param>
    private void SwapItems(InventorySlot targetSlot)
    {
        ItemData dataA = _draggedItemData;
        ItemData dataB = targetSlot.currentItem;
        GameObject objA = _draggedObject;
        GameObject objB = null;

        foreach(Transform child in targetSlot.transform)
        {
            if(child.CompareTag("DraggableItem"))
            {
                objB = child.gameObject;
                break;
            }
        }

        targetSlot.SetItem(dataA);
        _draggedFromSlot?.SetItem(dataB);

        if(objB != null)
        {
            objB.transform.SetParent(_draggedFromSlot != null ? _draggedFromSlot.transform : null);
            if(_draggedFromSlot != null) objB.transform.localPosition = Vector3.zero;
        }
        SnapToSlot(objA, targetSlot);
    }

    /// <summary>
    /// 尝试在目标槽位进行合成操作
    /// </summary>
    /// <param name="targetSlot">目标槽位</param>
    /// <returns>如果合成功则返回true</returns>
    private bool TryCrafting(InventorySlot targetSlot)
    {
        ItemData itemA = _draggedItemData;
        ItemData itemB = targetSlot.currentItem;

        foreach (var rule in craftingRules)
        {
            bool match1 = (rule.baseItem == itemA && rule.secondaryItem == itemB);
            bool match2 = (rule.baseItem == itemB && rule.secondaryItem == itemA);

            if (match1 || match2)
            {
                if (_draggedFromSlot != null) _draggedFromSlot.ClearItem();
                targetSlot.ClearItem();

                if (_draggedObject != null) Destroy(_draggedObject);
                foreach (Transform child in targetSlot.transform)
                {
                    if (child.CompareTag("DraggableItem") || child.gameObject.GetComponent<ItemData>() != null)
                    {
                        Destroy(child.gameObject);
                        break;
                    }
                }

                if (rule.resultItem.prefab != null)
                {
                    GameObject newItem = Instantiate(rule.resultItem.prefab, targetSlot.transform.position, Quaternion.identity);
                    newItem.transform.SetParent(targetSlot.transform);
                    newItem.transform.localPosition = Vector3.zero;
                    newItem.transform.localRotation = Quaternion.identity;
                    targetSlot.SetItem(rule.resultItem);
                }
                return true;
            }
        }
        return false;
    }
}
