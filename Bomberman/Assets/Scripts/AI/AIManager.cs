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
        var min = _map.MapSize.x * _map.MapSize.y;
        while (origin != target && path.Count < MAX_PATH_LENGTH)
        {
            var neighbours = GetNeighbours(target, true, false);

            //if (neighbours.Count == 0)
            //    neighbours = GetNeighbours(target, true, false);

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
                motion.y++;
                break;
            case EDirection.Down:
                motion.y--;
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

    private Dictionary<EDirection, Vector2Int> GetNeighbours(
        Vector2Int position, 
        bool onlyAccessible = false, 
        bool onlySafe = false)
    {
        var neighbours = new Dictionary<EDirection, Vector2Int>();

        var topPosition = new Vector2Int(position.x, position.y + 1);
        var bottomPosition = new Vector2Int(position.x, position.y - 1);
        var rightPosition = new Vector2Int(position.x + 1, position.y);
        var leftPosition = new Vector2Int(position.x - 1, position.y);

        var topIsAccessible = !onlyAccessible || (onlyAccessible && _map.IsAccessible(topPosition, false));
        var topIsSafe = !onlySafe || (onlySafe && _map.IsSafe(topPosition));
        var bottomIsAccessible = !onlyAccessible || (onlyAccessible && _map.IsAccessible(bottomPosition, false));
        var bottomIsSafe = !onlySafe || (onlySafe && _map.IsSafe(bottomPosition));
        var rightIsAccessible = !onlyAccessible || (onlyAccessible && _map.IsAccessible(rightPosition, false));
        var rightIsSafe = !onlySafe || (onlySafe && _map.IsSafe(rightPosition));
        var leftIsAccessible = !onlyAccessible || (onlyAccessible && _map.IsAccessible(leftPosition, false));
        var leftIsSafe = !onlySafe || (onlySafe && _map.IsSafe(leftPosition));

        if (!_map.IsOutOfBound(topPosition) && topIsAccessible && topIsSafe)
            neighbours.Add(EDirection.Up, topPosition);

        if (!_map.IsOutOfBound(bottomPosition) && bottomIsAccessible && bottomIsSafe)
            neighbours.Add(EDirection.Down, bottomPosition);

        if (!_map.IsOutOfBound(rightPosition) && rightIsAccessible && rightIsSafe)
            neighbours.Add(EDirection.Right, rightPosition);

        if (!_map.IsOutOfBound(leftPosition) && leftIsAccessible && leftIsSafe)
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
        var processedCells = new List<Vector2Int>();
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

                    if (!processedCells.Contains(cellPosition))
                    {
                        goalMatrix[cellPosition.x, cellPosition.y] = ComputeGoalValue(cellPosition, goalValue);
                        queue.Enqueue(cellPosition);

                        processedCells.Add(cellPosition);
                    }
                }

                goalValue++;
            }
        }

        goalMatrix[origin.x, origin.y] = 0;

        return goalMatrix;
    }

    private int ComputeGoalValue(Vector2Int cellPosition, int currentGoalValue)
    {
        int goalValue = 0;

        if (_map.GetDangerLevel(cellPosition) > 0)
            return goalValue;

        if (_map.GetEntityType(cellPosition) == EEntityType.None)
        {
            int wallsCount = GetAroundWallsCount(cellPosition);

            if (wallsCount > 0)
                goalValue = wallsCount; // max = 3
        }
        else if (_map.GetEntityType(cellPosition) == EEntityType.Bonus)
        {
            goalValue = 5;
        }
        else if (_map.GetEntityType(cellPosition) == EEntityType.Player)
        {
            goalValue = 4;
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

    public Vector2Int? FindNearestSafeCell(Vector2Int origin)
    {
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(origin);

        while (queue.Count > 0)
        {
            int counter = queue.Count;

            for (int i = 0; i < counter; i++)
            {
                var currentPosition = queue.Dequeue();

                Dictionary<EDirection, Vector2Int> neighbours = GetNeighbours(currentPosition, true, false);

                foreach (var neighbour in neighbours)
                {
                    var cellPosition = neighbour.Value;

                    if (_map.GetDangerLevel(cellPosition) == 0)
                        return cellPosition;

                    queue.Enqueue(cellPosition);
                }
            }
        }

        return null;
    }

    public bool IsSafe(Vector2Int cellPosition)
    {
        return _map.IsSafe(cellPosition);
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
