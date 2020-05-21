using System.Collections.Generic;
using UnityEngine;

public static class AIUtils
{
    public static Dictionary<EDirection, Vector2Int> GetNeighbours(
        Vector2Int position,
        Map map,
        bool onlyAccessible = false,
        bool checkBombs = false,
        bool checkExplosions = false)
    {
        var neighbours = new Dictionary<EDirection, Vector2Int>();

        var topPosition = new Vector2Int(position.x, position.y + 1);
        var bottomPosition = new Vector2Int(position.x, position.y - 1);
        var rightPosition = new Vector2Int(position.x + 1, position.y);
        var leftPosition = new Vector2Int(position.x - 1, position.y);

        var topIsAccessible = !onlyAccessible || map.IsAccessible(topPosition, checkBombs, checkExplosions);
        var bottomIsAccessible = !onlyAccessible || map.IsAccessible(bottomPosition, checkBombs, checkExplosions);
        var rightIsAccessible = !onlyAccessible || map.IsAccessible(rightPosition, checkBombs, checkExplosions);
        var leftIsAccessible = !onlyAccessible || map.IsAccessible(leftPosition, checkBombs, checkExplosions);

        if (!map.IsOutOfBound(topPosition) && topIsAccessible)
            neighbours.Add(EDirection.Up, topPosition);

        if (!map.IsOutOfBound(bottomPosition) && bottomIsAccessible)
            neighbours.Add(EDirection.Down, bottomPosition);

        if (!map.IsOutOfBound(rightPosition) && rightIsAccessible)
            neighbours.Add(EDirection.Right, rightPosition);

        if (!map.IsOutOfBound(leftPosition) && leftIsAccessible)
            neighbours.Add(EDirection.Left, leftPosition);

        return neighbours;
    }
}
