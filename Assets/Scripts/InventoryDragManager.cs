using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryDragManager : MonoBehaviour
{
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
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        if (mainCamera == null) mainCamera = Camera.main;
        
        slots = FindObjectsOfType<InventorySlot>();
        System.Array.Sort(slots, (a, b) => string.Compare(a.name, b.name));
    }

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

    private bool IsDraggableItem(Collider col)
    {
        return col.CompareTag("DraggableItem") ||
               (col.transform.parent != null && col.transform.parent.CompareTag("DraggableItem"));
    }

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

    private void HandleRotation()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame && _draggedObject != null)
        {
            _draggedObject.transform.Rotate(0, 90, 0, Space.World);
        }
    }

    private void CheckDrop()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            PerformDrop();
        }
    }

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
    
    private int GetSlotIndex(InventorySlot slot)
    {
        for(int i=0; i<slots.Length; i++)
            if(slots[i]==slot) return i;
        return -1;
    }

    private InventorySlot GetSlotByIndex(int idx)
    {
        if(slots==null || idx<0 || idx>=slots.Length) return null;
        return slots[idx];
    }

    private void ReturnToSlot(InventorySlot originalSlot)
    {
        SnapToSlot(_draggedObject, originalSlot);
    }

    private void SnapToSlot(GameObject obj, InventorySlot slot)
    {
        obj.transform.SetParent(slot.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

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