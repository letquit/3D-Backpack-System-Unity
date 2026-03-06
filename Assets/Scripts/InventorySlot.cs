using UnityEngine;

/// <summary>
/// 表示一个库存槽位组件，用于管理物品放置和视觉反馈
/// </summary>
[RequireComponent(typeof(Renderer))]
public class InventorySlot : MonoBehaviour
{
    /// <summary>
    /// 当前槽位中存放的物品数据
    /// </summary>
    public ItemData currentItem;
    
    /// <summary>
    /// 占据此槽位的游戏对象
    /// </summary>
    public GameObject occupyingObject;
    
    /// <summary>
    /// 渲染器组件引用
    /// </summary>
    private Renderer _renderer;

    /// <summary>
    /// 槽位的普通状态材质
    /// </summary>
    [SerializeField] private Material normalMaterial;
    
    /// <summary>
    /// 槽位的悬停状态材质
    /// </summary>
    [SerializeField] private Material hoverMaterial;
    
    /// <summary>
    /// 当前使用的材质
    /// </summary>
    private Material _currentMaterial;

    /// <summary>
    /// 获取当前槽位是否被悬停
    /// </summary>
    public bool IsHovered { get; private set; }

    /// <summary>
    /// 初始化组件，在对象创建时调用
    /// </summary>
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        SetMaterial(normalMaterial);
    }

    /// <summary>
    /// 设置槽位中的物品
    /// </summary>
    /// <param name="item">要设置的物品数据</param>
    /// <param name="obj">占据槽位的游戏对象（可选）</param>
    public void SetItem(ItemData item, GameObject obj = null)
    {
        currentItem = item;
        occupyingObject = obj;
        UpdateVisuals();
    }

    /// <summary>
    /// 清空槽位中的物品
    /// </summary>
    public void ClearItem()
    {
        currentItem = null;
        occupyingObject = null;
        UpdateVisuals();
    }

    /// <summary>
    /// 设置槽位的悬停状态
    /// </summary>
    /// <param name="hovered">是否悬停</param>
    public void SetHover(bool hovered)
    {
        IsHovered = hovered;
        SetMaterial(hovered ? hoverMaterial : normalMaterial);
    }

    /// <summary>
    /// 设置渲染器的材质
    /// </summary>
    /// <param name="newMaterial">新的材质</param>
    private void SetMaterial(Material newMaterial)
    {
        // 检查渲染器和材质是否为空
        if (_renderer == null || newMaterial == null) return;
        
        // 只在材质发生变化时更新
        if (_currentMaterial != newMaterial)
        {
            _renderer.material = newMaterial;
            _currentMaterial = newMaterial;
        }
    }

    /// <summary>
    /// 更新槽位的视觉表现
    /// </summary>
    private void UpdateVisuals()
    {
        
    }
}
