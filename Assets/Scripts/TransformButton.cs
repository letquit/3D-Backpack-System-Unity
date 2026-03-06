using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class TransformButton : MonoBehaviour
{
    [SerializeField] private bool isDisabled;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Material pressedMaterial;
    [SerializeField] private Material disabledMaterial;
    [SerializeField] private LayerMask layer;

    private Renderer _renderer;
    private Camera _mainCamera;
    private Material _currentMaterial;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _mainCamera = Camera.main;

        if (_renderer != null)
        {
            SetMaterial(isDisabled ? disabledMaterial : normalMaterial);
        }
    }

    private void Update()
    {
        if (_renderer == null || _mainCamera == null) return;

        if (isDisabled)
        {
            SetMaterial(disabledMaterial);
            return;
        }

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