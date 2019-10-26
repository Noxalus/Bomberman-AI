using System;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    #region Constants

    private const int MAX_PATH_LENGTH = 1000;

    #endregion

    #region Private fields

    private Map _map = null;
    private List<AIPlayer> _aiPlayers = new List<AIPlayer>();
    private Vector2Int _areaSize = Vector2Int.zero;

    #endregion

    #region Properties

    public Vector2Int AreaSize => _areaSize;
    public List<AIPlayer> AIPlayers => _aiPlayers;

    #endregion

    public void Initialize(Map map, List<AIPlayer> aiPlayers)
    {
        _map = map;
        _aiPlayers = aiPlayers;

        _areaSize = _map.MapSize;

        foreach (var aiPlayer in aiPlayers)
        {
            aiPlayer.Behaviour.Initialize(this);
        }
    }

    public void Clear()
    {
        foreach (var aiPlayer in _aiPlayers)
            Destroy(aiPlayer.gameObject);

        _aiPlayers.Clear();
    }


    public Stack<Vector2Int> ComputePath(Vector2Int origin, Vector2Int target)
    {
        int[,] costMap = ComputeCostMap(origin);

        var path = new Stack<Vector2Int>();
        path.Push(target);

        var direction = EDirection.None;
        while (origin != target && path.Count < MAX_PATH_LENGTH)
        {
            var min = _map.MapSize.x * _map.MapSize.y;

            var neighbours = GetNeighbours(target);

            foreach (var neighbourPair in neighbours)
            {
                if (costMap[neighbourPair.Value.x, neighbourPair.Value.y] < min)
                {
                    min = costMap[neighbourPair.Value.x, neighbourPair.Value.y];
                    direction = neighbourPair.Key;
                }
            }

            target += DirectionToMotion(direction);

            // No way to reach the target
            if (target == path.Peek())
                return new Stack<Vector2Int>();

            if (target != origin)
                path.Push(target);
        }

        return path;
    }

    private Vector2Int DirectionToMotion(EDirection direction)
    {
        var motion = Vector2Int.zero;

        switch (direction)
        {
            case EDirection.Up:
                motion.y--;
                break;
            case EDirection.Down:
                motion.y++;
                break;
            case EDirection.Right:
                motion.x++;
                break;
            case EDirection.Left:
                motion.x--;
                break;
        }

        return motion;
    }

    private Dictionary<EDirection, Vector2Int> GetNeighbours(Vector2Int position, bool onlyAccessible = false)
    {
        var neighbours = new Dictionary<EDirection, Vector2Int>();

        var topPosition = new Vector2Int(position.x, position.y - 1);
        var bottomPosition = new Vector2Int(position.x, position.y + 1);
        var rightPosition = new Vector2Int(position.x + 1, position.y);
        var leftPosition = new Vector2Int(position.x - 1, position.y);

        if (!_map.IsOutOfBound(topPosition) && (!onlyAccessible || (onlyAccessible && _map.IsAccessible(topPosition))))
            neighbours.Add(EDirection.Up, topPosition);

        if (!_map.IsOutOfBound(bottomPosition) && (!onlyAccessible || (onlyAccessible && _map.IsAccessible(bottomPosition))))
            neighbours.Add(EDirection.Down, bottomPosition);

        if (!_map.IsOutOfBound(rightPosition) && (!onlyAccessible || (onlyAccessible && _map.IsAccessible(rightPosition))))
            neighbours.Add(EDirection.Right, rightPosition);

        if (!_map.IsOutOfBound(leftPosition) && (!onlyAccessible || (onlyAccessible && _map.IsAccessible(leftPosition))))
            neighbours.Add(EDirection.Left, leftPosition);

        return neighbours;
    }

    public int[,] ComputeCostMap(Vector2Int targetPosition)
    {
        var costMatrix = new int[AreaSize.x, AreaSize.y];

        // We put all cells at the "infinity" value
        for (int x = 0; x < AreaSize.x; x++)
            for (int y = 0; y < AreaSize.y; y++)
                costMatrix[x, y] = AreaSize.x * AreaSize.y;

        int cellValue = 0;
        var queue = new Queue<Vector2Int>();

        costMatrix[targetPosition.x, targetPosition.y] = cellValue;
        queue.Enqueue(targetPosition);

        while (queue.Count > 0)
        {
            cellValue++;
            int counter = queue.Count;
            for (int i = 0; i < counter; i++)
            {
                var currentPosition = queue.Dequeue();

                Dictionary<EDirection, Vector2Int> neighbours = GetNeighbours(currentPosition, true);

                foreach (var neighbour in neighbours)
                {
                    var cellPosition = neighbour.Value;
                    if (costMatrix[cellPosition.x, cellPosition.y] > cellValue)
                    {
                        costMatrix[cellPosition.x, cellPosition.y] = cellValue;
                        queue.Enqueue(cellPosition);
                    }
                }
            }
        }

        return costMatrix;
    }

    public int[,] ComputeGoalMap(Vector2Int origin)
    {
        var goalMatrix = new int[AreaSize.x, AreaSize.y];
        var queue = new Queue<Vector2Int>();
        int goalValue = 0;

        queue.Enqueue(origin);

        while (queue.Count > 0)
        {
            int counter = queue.Count;

            for (int i = 0; i < counter; i++)
            {
                var currentPosition = queue.Dequeue();

                Dictionary<EDirection, Vector2Int> neighbours = GetNeighbours(currentPosition, true);

                foreach (var neighbour in neighbours)
                {
                    var cellPosition = neighbour.Value;
                    if (goalMatrix[cellPosition.x, cellPosition.y] == 0)
                    {
                        goalMatrix[cellPosition.x, cellPosition.y] = ComputeGoalValue(cellPosition, goalValue);
                        queue.Enqueue(cellPosition);
                    }
                }

                goalValue++;
            }
        }

        goalMatrix[origin.x, origin.y] = -1;

        return goalMatrix;
    }

    private int ComputeGoalValue(Vector2Int cellPosition, int currentGoalValue)
    {
        int goalValue = 0;

        if (_map.GetEntityType(cellPosition) == EEntityType.None)
        {
            int wallsCount = GetAroundWallsCount(cellPosition);

            if (wallsCount > 0)
            {
                goalValue = Mathf.Clamp(
                    ((AreaSize.x * AreaSize.y) / 2) - currentGoalValue + wallsCount,
                    wallsCount,
                    (AreaSize.x * AreaSize.y) / 2 + 4);
            }
            else
            {
                goalValue = (int)Mathf.Clamp(currentGoalValue, 1f, (AreaSize.x * AreaSize.y) - currentGoalValue - 10 - 1);
                //goalValue = 1;
            }
        }
        else if (_map.GetEntityType(cellPosition) == EEntityType.Bonus)
        {
            goalValue = (AreaSize.x * AreaSize.y) - currentGoalValue;
        }
        else if (_map.GetEntityType(cellPosition) == EEntityType.Player)
        {
            goalValue = (AreaSize.x * AreaSize.y) - currentGoalValue - 10;
        }

        return goalValue;
    }

    public Vector2Int? GetBestGoalPosition(Vector2Int origin)
    {
        int[,] goalMatrix = ComputeGoalMap(origin);

        Vector2Int? cellPosition = null;
        int max = 0;

        for (int x = 0; x < goalMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < goalMatrix.GetLength(1); y++)
            {
                if (goalMatrix[x, y] > max)
                {
                    max = goalMatrix[x, y];

                    cellPosition = new Vector2Int(x, y);
                }
            }
        }

        return cellPosition;
    }


    private int GetAroundWallsCount(Vector2Int cellPosition)
    {
        int wallsCount = 0;
        Dictionary<EDirection, Vector2Int> neighbours = GetNeighbours(cellPosition);

        foreach (var neighbour in neighbours)
        {
            if (_map.GetEntityType(neighbour.Value) == EEntityType.DestructibleWall)
                wallsCount++;
        }

        return wallsCount;
    }

    public bool IsAccessible(Vector2Int cellPosition)
    {
        return _map.IsAccessible(cellPosition);
    }

    public Vector3 WorldPosition(Vector2Int cellPosition)
    {
        return _map.WorldPosition(cellPosition);
    }

    public Vector2Int CellPosition(Vector3 worldPosition)
    {
        return _map.CellPosition(worldPosition);
    }
}
