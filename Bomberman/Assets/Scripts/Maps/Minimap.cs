using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [SerializeField]
    private Vector2Int _size = new Vector2Int(150, 150);

    protected Map _map;

    List<Image> _cellImages = new List<Image>();
    private GridLayoutGroup _gridLayout = null;
    private LayoutElement _layoutElement = null;

    public void Initialize(Map map)
    {
        _map = map;

        int maxSize = Mathf.Max(_map.MapSize.x, _map.MapSize.y);

        _gridLayout = gameObject.AddComponent<GridLayoutGroup>();
        _gridLayout.childAlignment = TextAnchor.MiddleCenter;
        _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayout.constraintCount = _map.MapSize.x;
        _gridLayout.cellSize = new Vector2(_size.x / maxSize, _size.y / maxSize);
        _gridLayout.spacing = new Vector2(-_gridLayout.cellSize.x, -_gridLayout.cellSize.y);
        _gridLayout.padding.left = (int)-_gridLayout.cellSize.x;
        _gridLayout.padding.right = (int)-_gridLayout.cellSize.x;
        _gridLayout.padding.top = (int)-_gridLayout.cellSize.y;
        _gridLayout.padding.bottom = (int)-_gridLayout.cellSize.y;

        _gridLayout.cellSize *= 2;

        _layoutElement = gameObject.AddComponent<LayoutElement>();
        _layoutElement.preferredWidth = _size.x;
        _layoutElement.preferredHeight = _size.y;

        InstantiateCells();
    }

    public void Clear()
    {
        foreach (var cellImage in _cellImages)
            Destroy(cellImage.gameObject);

        _cellImages.Clear();

        Destroy(_gridLayout);
        _gridLayout = null;

        Destroy(_layoutElement);
        _layoutElement = null;
    }

    public void InstantiateCells()
    {
        for (int y = 0; y < _map.MapSize.y; y++)
        {
            for (int x = 0; x < _map.MapSize.x; x++)
            {
                GameObject cell = new GameObject($"MinimapCell[{x},{y}]");
                cell.transform.SetParent(transform);
                Image cellImage = cell.AddComponent<Image>();

                _cellImages.Add(cellImage);
            }
        }
    }

    public Image GetCell(int x, int y)
    {
        if (x + y * _map.MapSize.x > _cellImages.Count)
        {
            Debug.LogError("Error");
        }

        return _cellImages[x + y * _map.MapSize.x];
    }
}
