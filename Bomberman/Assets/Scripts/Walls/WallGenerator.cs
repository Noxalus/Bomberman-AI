using UnityEngine;

public class WallGenerator : MonoBehaviour
{
    private Map _map = null;

    public void Initialize(Map map)
    {
        _map = map;
    }

    public void GenerateWalls(float wallPercentage, Transform holder)
    {
        var wallOffset = new Vector3(0.5f, 1f, 0f);

        for (int y = 0; y <= _map.MapSize.y; y++)
        {
            for (int x = 0; x <= _map.MapSize.x; x++)
            {
                var normalizedCellPosition = new Vector2Int(x, y);
                var cellPosition = _map.GetCellPositionFromNormalizedPosition(normalizedCellPosition);
                var tile = _map.GameTilemap.GetTile(cellPosition);

                if (tile)
                {
                    var worldPosition = _map.GameGrid.CellToWorld(cellPosition) + _map.GameTilemap.tileAnchor;

                    if (_map.OverlapPlayerSpawn(cellPosition) || _map.OverlapWall(worldPosition))
                    {
                        continue;
                    }

                    if (Random.value < wallPercentage)
                    {
                        _map.AddDestructibleWall(worldPosition);
                    }
                }
            }
        }
    }
}
