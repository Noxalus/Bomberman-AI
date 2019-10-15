using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    #region Constants

    private const int MAX_PATH_LENGTH = 1000;

    #endregion

    #region Serialized fields

    // Debug
    [SerializeField] private TextMeshProUGUI _costMapText = null;

    #endregion

    #region Private fields

    private Map _map = null;
    private List<AIPlayer> _aiPlayers = new List<AIPlayer>();
    private Vector2Int _areaSize = Vector2Int.zero;

    #endregion

    #region Properties

    public Vector2Int AreaSize => _areaSize;

    #endregion

    public void Initialize(Map map, List<AIPlayer> aiPlayers)
    {
        _map = map;
        _aiPlayers = aiPlayers;

        _areaSize = _map.MapSize;

        foreach (var aiPlayer in aiPlayers)
            aiPlayer.Behaviour.Initialize(this);
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

    private Dictionary<EDirection, Vector2Int> GetNeighbours(Vector2Int position)
    {
        var neighbours = new Dictionary<EDirection, Vector2Int>();

        var topPosition = new Vector2Int(position.x, position.y - 1);
        if (!_map.IsNormalizedCellPositionOutOfBound(topPosition))
            neighbours.Add(EDirection.Up, topPosition);

        var bottomPosition = new Vector2Int(position.x, position.y + 1);
        if (!_map.IsNormalizedCellPositionOutOfBound(bottomPosition))
            neighbours.Add(EDirection.Down, bottomPosition);

        var rightPosition = new Vector2Int(position.x + 1, position.y);
        if (!_map.IsNormalizedCellPositionOutOfBound(rightPosition))
            neighbours.Add(EDirection.Right, rightPosition);

        var leftPosition = new Vector2Int(position.x - 1, position.y);
        if (!_map.IsNormalizedCellPositionOutOfBound(leftPosition))
            neighbours.Add(EDirection.Left, leftPosition);

        return neighbours;
    }

    public int[,] ComputeCostMap(Vector2Int targetNormalizedCellPosition)
    {
        var costMatrix = new int[AreaSize.x, AreaSize.y];

        // We put all cells at the "infinity" value
        for (int x = 0; x < AreaSize.x; x++)
            for (int y = 0; y < AreaSize.y; y++)
                costMatrix[x, y] = AreaSize.x * AreaSize.y;

        int id = 0;
        var queue = new Queue<Vector2Int>();

        costMatrix[targetNormalizedCellPosition.x, targetNormalizedCellPosition.y] = id;
        queue.Enqueue(targetNormalizedCellPosition);

        while (queue.Count > 0)
        {
            id++;
            int counter = queue.Count;
            for (int i = 0; i < counter; i++)
            {
                var currentPosition = queue.Dequeue();

                // Top
                if (currentPosition.y - 1 >= 0 && IsAccessible(new Vector2Int(currentPosition.x, currentPosition.y - 1)) &&
                    costMatrix[currentPosition.x, currentPosition.y - 1] > id)
                {
                    costMatrix[currentPosition.x, currentPosition.y - 1] = id;
                    queue.Enqueue(new Vector2Int(currentPosition.x, currentPosition.y - 1));
                }
                // Right
                if (currentPosition.x + 1 < AreaSize.x && IsAccessible(new Vector2Int(currentPosition.x + 1, currentPosition.y)) &&
                    costMatrix[currentPosition.x + 1, currentPosition.y] > id)
                {
                    costMatrix[currentPosition.x + 1, currentPosition.y] = id;
                    queue.Enqueue(new Vector2Int(currentPosition.x + 1, currentPosition.y));
                }
                if (currentPosition.y + 1 < AreaSize.y && IsAccessible(new Vector2Int(currentPosition.x, currentPosition.y + 1)) &&
                    costMatrix[currentPosition.x, currentPosition.y + 1] > id)
                {
                    costMatrix[currentPosition.x, currentPosition.y + 1] = id;
                    queue.Enqueue(new Vector2Int(currentPosition.x, currentPosition.y + 1));
                }
                // Left
                if (currentPosition.x - 1 >= 0 && IsAccessible(new Vector2Int(currentPosition.x - 1, currentPosition.y)) &&
                    costMatrix[currentPosition.x - 1, currentPosition.y] > id)
                {
                    costMatrix[currentPosition.x - 1, currentPosition.y] = id;
                    queue.Enqueue(new Vector2Int(currentPosition.x - 1, currentPosition.y));
                }
            }
        }

        // To debug only
        DrawCostMap(costMatrix);

        return costMatrix;
    }

    private void DrawCostMap(int[,] costMatrix)
    {
        StringBuilder costMapString = new StringBuilder();

        for (int y = 0; y < AreaSize.y; y++)
        {
            for (int x = 0; x < AreaSize.x; x++)
            {
                string c = "_";

                var yDown = (AreaSize.y - 1) - y;
                if (costMatrix[x, yDown] >= 0 && costMatrix[x, yDown] < 26)
                    c = char.ConvertFromUtf32(97 + costMatrix[x, yDown]);
                else if (costMatrix[x, yDown] >= AreaSize.x * AreaSize.y)
                    c = "X";

                costMapString.Append(c + " ");
            }

            costMapString.Append("\n");
        }

        _costMapText.text = costMapString.ToString();
    }

    public bool IsAccessible(Vector2Int normalizedCellPosition)
    {
        return !_map.IsNormalizedCellPositionOutOfBound(normalizedCellPosition) &&
                _map.GetEntityType(normalizedCellPosition) != EEntityType.UnbreakableWall &&
                _map.GetEntityType(normalizedCellPosition) != EEntityType.DestructibleWall &&
                _map.GetEntityType(normalizedCellPosition) != EEntityType.Explosion &&
                _map.GetEntityType(normalizedCellPosition) != EEntityType.Bomb;
    }

    public Vector3 WorldPosition(Vector2Int normalizeCellPosition)
    {
        return _map.GetWorldPositionFromNormalizedPosition(normalizeCellPosition);
    }

    // Actually returns normalized cell position
    public Vector2Int CellPosition(Vector3 worldPosition)
    {
        return _map.GetNormalizedCellPositionFromWorldPosition(worldPosition);
    }
}
