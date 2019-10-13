using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public enum Direction
{
    None = 0,
    Up = 1,
    Right = 2,
    Down = 3,
    Left = 4
}

public class NormalAIBehaviour : MonoBehaviour
{
    [Header("Inner references")]

    [SerializeField] private Player _player = null;
    [SerializeField] private Rigidbody2D _rigidbody = null;
    [SerializeField] private Animator _animator = null;

    // Debug
    [SerializeField] private TextMeshProUGUI _costMatrixText = null;

    [Header("Assets")]

    [SerializeField] private GameSettings _gameSettings = null;

    private Vector2 _movement = Vector2.zero;
    private Vector3 _target = Vector3.zero;
    private bool _isMovingToTarget = false;
    private List<Vector2Int> _currentPath = new List<Vector2Int>();

    // TO REMOVE: Shoud be handled by the AIManager
    private Map _map = null;

    public void Initialize(Map map)
    {
        _map = map;
    }

    private void Update()
    {
        if (_isMovingToTarget)
        {

        }
        else
        {
            _target = SearchNextTarget();
            var targetNormalizedPath = _map.GetNormalizedCellPositionFromWorldPosition(_target);
            _currentPath = ComputePath(targetNormalizedPath);
            _isMovingToTarget = true;
        }

        _animator.SetFloat("Horizontal", _movement.x);
        _animator.SetFloat("Vertical", _movement.y);

        bool isMoving = _movement.sqrMagnitude > 0;

        if (isMoving)
        {
            _animator.SetFloat("PreviousHorizontal", _movement.x);
            _animator.SetFloat("PreviousVertical", _movement.y);
        }

        _animator.SetBool("IsMoving", isMoving);
    }

    private Vector3 SearchNextTarget()
    {
        // TODO: Add cost matrix logic here
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x <= _map.MapSize.x; x++)
        {
            for (int y = 0; y <= _map.MapSize.y; y++)
            {
                var normalizedCellPosition = new Vector2Int(x, y);
                if (_map.GetEntityType(normalizedCellPosition) == EEntityType.None)
                {
                    availablePositions.Add(normalizedCellPosition);
                }
            }
        }

        var randomNormalizedCellPosition = availablePositions[Random.Range(0, availablePositions.Count)];
        var randomCellPosition = _map.GetCellPositionFromNormalizedPosition(randomNormalizedCellPosition);

        return _map.GameGrid.CellToWorld(randomCellPosition);
    }

    private List<Vector2Int> ComputePath(Vector2Int target)
    {
        var origin = _map.GetNormalizedCellPositionFromWorldPosition(transform.position);
        int[,] costMatrix = ComputeCostMatrix(origin);

        var path = new List<Vector2Int> { target };

        var direction = Direction.None;
        while (origin != target && path.Count < 100)
        {
            var min = _map.MapSize.x * _map.MapSize.y;

            var neighbours = GetNeighbours(target);

            foreach (var neighbourPair in neighbours)
            {
                if (costMatrix[neighbourPair.Value.x, neighbourPair.Value.y] < min)
                {
                    min = costMatrix[neighbourPair.Value.x, neighbourPair.Value.y];
                    direction = neighbourPair.Key;
                }
            }

            switch (direction)
            {
                case Direction.Up:
                    target.y--;
                    break;
                case Direction.Down:
                    target.y++;
                    break;
                case Direction.Right:
                    target.x++;
                    break;
                case Direction.Left:
                    target.x--;
                    break;
            }

            if (target != origin)
                path.Add(target);
        }

        return path;
    }

    private Dictionary<Direction, Vector2Int> GetNeighbours(Vector2Int position)
    {
        var neighbours = new Dictionary<Direction, Vector2Int>();

        var topPosition = new Vector2Int(position.x, position.y - 1);
        if (_map.IsNormalizedCellPositionOutOfBound(topPosition))
            neighbours.Add(Direction.Up, topPosition);

        var bottomPosition = new Vector2Int(position.x, position.y + 1);
        if (_map.IsNormalizedCellPositionOutOfBound(bottomPosition))
            neighbours.Add(Direction.Down, bottomPosition);

        var rightPosition = new Vector2Int(position.x + 1, position.y);
        if (_map.IsNormalizedCellPositionOutOfBound(rightPosition))
            neighbours.Add(Direction.Right, rightPosition);

        var leftPosition = new Vector2Int(position.x - 1, position.y);
        if (_map.IsNormalizedCellPositionOutOfBound(leftPosition))
            neighbours.Add(Direction.Left, leftPosition);

        return neighbours;
    }

    public int[,] ComputeCostMatrix(Vector2Int targetNormalizedCellPosition)
    {
        var costMatrix = new int[_map.MapSize.x + 1, _map.MapSize.y + 1];

        // We put all cells at the "infinity" value
        for (int x = 0; x < _map.MapSize.x; x++)
            for (int y = 0; y < _map.MapSize.y; y++)
                costMatrix[x, y] = _map.MapSize.x * _map.MapSize.y;

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
                if (currentPosition.x + 1 < _map.MapSize.x && IsAccessible(new Vector2Int(currentPosition.x + 1, currentPosition.y)) &&
                    costMatrix[currentPosition.x + 1, currentPosition.y] > id)
                {
                    costMatrix[currentPosition.x + 1, currentPosition.y] = id;
                    queue.Enqueue(new Vector2Int(currentPosition.x + 1, currentPosition.y));
                }
                // Bottom
                if (currentPosition.y + 1 < _map.MapSize.y && IsAccessible(new Vector2Int(currentPosition.x, currentPosition.y + 1)) &&
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
        DrawCostMatrix(costMatrix);

        return costMatrix;
    }
    private void DrawCostMatrix(int[,] costMatrix)
    {
        StringBuilder costMatrixString = new StringBuilder();

        for (int y = 0; y <= _map.MapSize.y; y++)
        {
            for (int x = 0; x <= _map.MapSize.x; x++)
            {
                costMatrixString.Append(costMatrix[x, _map.MapSize.y - y] + " ");
            }

            costMatrixString.Append("\n");
        }

        _costMatrixText.text = costMatrixString.ToString();
    }


    private bool IsAccessible(Vector2Int normalizedCellPosition)
    {
        return (_map.GetEntityType(normalizedCellPosition) != EEntityType.UnbreakableWall ||
                _map.GetEntityType(normalizedCellPosition) != EEntityType.DestructibleWall ||
                _map.GetEntityType(normalizedCellPosition) != EEntityType.Explosion ||
                _map.GetEntityType(normalizedCellPosition) != EEntityType.Bomb);
    }

    private void FixedUpdate()
    {
        float speed = _gameSettings.PlayerBaseSpeed + (_player.SpeedBonus * _gameSettings.SpeedBonusIncrement);
        _rigidbody.MovePosition(_rigidbody.position + _movement * speed * Time.fixedDeltaTime);
    }
}
