using UnityEngine;
using UnityEngine.Tilemaps;

public class WallGenerator : MonoBehaviour
{
    [Header("Scene references")]

    [SerializeField] private Transform _destructibleWallHolder = null;
    [SerializeField] private Grid _gameGrid = null;
    [SerializeField] private Tilemap _gameTilemap = null;
    [SerializeField] private TilemapCollider2D _collisionMap = null;

    [Header("Assets references")]

    [SerializeField] private DestructibleWall _destructiveWallPrefab = null;

    public void GenerateWalls(float wallPercentage)
    {
        var wallOffset = new Vector3(0.5f, 1f, 0f);
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        for (int y = _gameTilemap.origin.y; y < _gameTilemap.size.y; y++)
        {
            for (int x = _gameTilemap.origin.x; x < _gameTilemap.size.x; x++)
            {
                var cellPosition = new Vector3Int(x, -y, 0);
                var tile = _gameTilemap.GetTile(cellPosition);

                if (tile)
                {
                    //TileData tileData;
                    //tile.GetTileData(cellPosition, _collisionMap, tileData);
                    //tileData.colliderType == Tile.ColliderType.Grid;

                    var worldPosition = _gameGrid.CellToWorld(cellPosition) + _gameTilemap.tileAnchor;

                    if (_collisionMap.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y)))
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
