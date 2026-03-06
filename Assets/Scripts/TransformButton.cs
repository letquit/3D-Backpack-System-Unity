using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 变换按钮组件，用于处理带有材质变化的交互式按钮功能
/// </summary>
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class TransformButton : MonoBehaviour
{
    /// <summary>
    /// 按钮是否被禁用
    /// </summary>
    [SerializeField] private bool isDisabled;
    
    /// <summary>
    /// 正常状态下的材质
    /// </summary>
    [SerializeField] private Material normalMaterial;
    
    /// <summary>
    /// 鼠标悬停时的材质
    /// </summary>
    [SerializeField] private Material hoverMaterial;
    
    /// <summary>
    /// 按钮按下时的材质
    /// </summary>
    [SerializeField] private Material pressedMaterial;
    
    /// <summary>
    /// 禁用状态下的材质
    /// </summary>
    [SerializeField] private Material disabledMaterial;
    
    /// <summary>
    /// 用于射线检测的层掩码
    /// </summary>
    [SerializeField] private LayerMask layer;

    private Renderer _renderer;
    private Camera _mainCamera;
    private Material _currentMaterial;

    /// <summary>
    /// 初始化组件，在游戏对象启动时设置渲染器和主相机引用
    /// </summary>
    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _mainCamera = Camera.main;

        if (_renderer != null)
        {
            SetMaterial(isDisabled ? disabledMaterial : normalMaterial);
        }
    }

    /// <summary>
    /// 更新按钮状态，根据鼠标交互和按钮状态切换对应材质
    /// </summary>
    private void Update()
    {
        if (_renderer == null || _mainCamera == null) return;

        if (isDisabled)
        {
            SetMaterial(disabledMaterial);
            return;
        }

        // 获取当前鼠标位置并创建射线
        Vector2 mousePos = Mouse.current?.position.ReadValue() ?? Input.mousePosition;
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layer))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                if (Mouse.current?.leftButton.isPressed == true)
                {
                    SetMaterial(pressedMaterial);
                }
                else
                {
                    SetMaterial(hoverMaterial);
                }
                return;
            }
        }

        SetMaterial(normalMaterial);
    }

    /// <summary>
    /// 设置按钮的材质，只有在材质发生变化时才更新
    /// </summary>
    /// <param name="newMaterial">要设置的新材质</param>
    private void SetMaterial(Material newMaterial)
    {
        if (newMaterial == null) return;
        if (_currentMaterial != newMaterial)
        {
            _renderer.material = newMaterial;
            _currentMaterial = newMaterial;
        }
    }
}
