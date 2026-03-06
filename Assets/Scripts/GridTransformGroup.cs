using System;
using UnityEngine;

[ExecuteAlways]
public class GridTransformGroup : MonoBehaviour
{
    [SerializeField] private int columns = 3;
    [SerializeField] private Vector2 cellSize = Vector2.one;
    [SerializeField] private Vector2 spacing = Vector2.one;

    private int prevColumns = -1;
    private Vector2 prevCellSize = Vector2.zero;
    private Vector2 prevSpacing = Vector2.zero;
    private int prevChildCount = -1;

    private void OnValidate()
    {
        if (columns < 1) columns = 1;
        ArrangeChildren();
    }

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

    public void ArrangeChildren()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
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

        prevColumns = columns;
        prevCellSize = cellSize;
        prevSpacing = spacing;
        prevChildCount = transform.childCount;
    }

    private void OnDrawGizmos()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

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