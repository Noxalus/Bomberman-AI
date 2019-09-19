using UnityEngine;
using UnityEngine.Tilemaps;

public class WallGenerator : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private Transform _destructibleWallHolder = null;

    [Header("Assets references")]
    [SerializeField] private DestructibleWall _destructiveWallPrefab = null;

    private Map _map = null;

    public void Initialize(Map map)
    {
        _map = map;
    }

    public void GenerateWalls(float wallPercentage)
    {
        var wallOffset = new Vector3(0.5f, 1f, 0f);
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        for (int y = _map.GameTilemap.origin.y; y < _map.GameTilemap.size.y; y++)
        {
            for (int x = _map.GameTilemap.origin.x; x < _map.GameTilemap.size.x; x++)
            {
                var cellPosition = new Vector3Int(x, -y, 0);
                var tile = _map.GameTilemap.GetTile(cellPosition);

                if (tile)
                {
                    var worldPosition = _map.GameGrid.CellToWorld(cellPosition) + _map.GameTilemap.tileAnchor;

                    if (_map.IsOverlappingPlayerSpawn(cellPosition) ||
                        _map.CollisionMap.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y)))
                    {
                        continue;
                    }

                    if (cellPosition.x < min.x)
                        min.x = cellPosition.x;
                    else if (cellPosition.x > max.x)
                        max.x = cellPosition.x;

                    if (cellPosition.y < min.y)
                        min.y = cellPosition.y;
                    else if (cellPosition.y > max.y)
                        max.y = cellPosition.y;

                    Debug.Log($"Current tile: {tile}");
                    Debug.Log($"World position: {worldPosition}");

                    Instantiate(
                        _destructiveWallPrefab,
                        worldPosition,
                        Quaternion.identity,
                        _destructibleWallHolder
                    );
                }
            }
        }

        Debug.Log($"Min: {min}");
        Debug.Log($"Max: {max}");
    }
}
