using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class InventorySlot : MonoBehaviour
{
    public ItemData currentItem;
    public GameObject occupyingObject;
    private Renderer _renderer;

    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material hoverMaterial;
    private Material _currentMaterial;

    public bool IsHovered { get; private set; }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        SetMaterial(normalMaterial);
    }

    public void SetItem(ItemData item, GameObject obj = null)
    {
        currentItem = item;
        occupyingObject = obj;
        UpdateVisuals();
    }

    public void ClearItem()
    {
        currentItem = null;
        occupyingObject = null;
        UpdateVisuals();
    }

    public void SetHover(bool hovered)
    {
        IsHovered = hovered;
        SetMaterial(hovered ? hoverMaterial : normalMaterial);
    }

    private void SetMaterial(Material newMaterial)
    {
        if (_renderer == null || newMaterial == null) return;
        if (_currentMaterial != newMaterial)
        {
            _renderer.material = newMaterial;
            _currentMaterial = newMaterial;
        }
    }

    private void UpdateVisuals()
    {
        
    }
}