using System;
using UnityEngine;

/// <summary>
/// 网格变换组组件，用于将子对象按照网格布局进行排列
/// </summary>
[ExecuteAlways]
public class GridTransformGroup : MonoBehaviour
{
    /// <summary>
    /// 网格列数
    /// </summary>
    [SerializeField] private int columns = 3;
    
    /// <summary>
    /// 每个网格单元的尺寸
    /// </summary>
    [SerializeField] private Vector2 cellSize = Vector2.one;
    
    /// <summary>
    /// 网格之间的间距
    /// </summary>
    [SerializeField] private Vector2 spacing = Vector2.one;

    /// <summary>
    /// 上一次记录的列数
    /// </summary>
    private int prevColumns = -1;
    
    /// <summary>
    /// 上一次记录的单元尺寸
    /// </summary>
    private Vector2 prevCellSize = Vector2.zero;
    
    /// <summary>
    /// 上一次记录的间距
    /// </summary>
    private Vector2 prevSpacing = Vector2.zero;
    
    /// <summary>
    /// 上一次记录的子对象数量
    /// </summary>
    private int prevChildCount = -1;

    /// <summary>
    /// 验证组件参数时调用，确保列数有效并重新排列子对象
    /// </summary>
    private void OnValidate()
    {
        if (columns < 1) columns = 1;
        ArrangeChildren();
    }

    /// <summary>
    /// 检测参数变化并在变化时重新排列子对象
    /// </summary>
    private void Update()
    {
        if (
            columns != prevColumns ||
            cellSize != prevCellSize ||
            spacing != prevSpacing ||
            transform.childCount != prevChildCount
        )
        {
            ArrangeChildren();
        }
    }

    /// <summary>
    /// 按照网格布局重新排列所有子对象
    /// </summary>
    public void ArrangeChildren()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                // 计算当前子对象在网格中的行列位置
                int row = i / columns;
                int col = i % columns;
                Vector3 newPosition = new
                (
                    col * (cellSize.x + spacing.x),
                    -row * (cellSize.y + spacing.y),
                    0
                );
                child.localPosition = newPosition;
            }
        }

        // 更新缓存的参数值
        prevColumns = columns;
        prevCellSize = cellSize;
        prevSpacing = spacing;
        prevChildCount = transform.childCount;
    }

    /// <summary>
    /// 绘制网格布局的调试线框
    /// </summary>
    private void OnDrawGizmos()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            // 计算网格位置
            int row = i / columns;
            int col = i % columns;
            Vector3 center = transform.position +
                new Vector3(
                    col * (cellSize.x + spacing.x),
                    -row * (cellSize.y + spacing.y),
                    0
                );

            Gizmos.color = Color.green;
            Vector3 size = new Vector3(cellSize.x, cellSize.y, 0.05f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
