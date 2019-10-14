using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

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

    // Debug
    [SerializeField] float _debugSpeedFactor = 0.25f;
    [SerializeField] private Sprite _debugSprite = null;

    private Vector2 _movement = Vector2.zero;
    private Vector3 _targetPosition = Vector3.zero;
    private bool _isMovingToTarget = false;
    private Stack<Vector2Int> _currentPath = new Stack<Vector2Int>();
    private Vector3 _nextPosition = Vector3.zero;
    private List<GameObject> _pathDebugSprites = new List<GameObject>();

    // TO REMOVE: Shoud be handled by the AIManager
    private Map _map = null;

    public void Initialize(Map map)
    {
        _map = map;
    }

    private void Update()
    {
        // Debug: Place a target position for the AI
        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Input.mousePosition;
            var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            var normalizedCellPosition = _map.GetNormalizedCellPositionFromWorldPosition(worldPosition);

            if (IsAccessible(normalizedCellPosition))
            {
                var cellPosition = _map.GetCellPositionFromNormalizedPosition(normalizedCellPosition);
                worldPosition = _map.CellToWorld(cellPosition);
                UpdateTarget(worldPosition);
            }
            else
            {
                Debug.LogWarning("Position not accessible for the bot...");
            }
        }

        if (_isMovingToTarget)
        {
            _movement = Vector2Int.zero;
            var distance = new Vector2(
                Mathf.Abs(transform.position.x - _nextPosition.x),
                Mathf.Abs(transform.position.y - _nextPosition.y)
            );

            if (distance.x > 0.1f || distance.y > 0.1f)
            {
                if (distance.x > distance.y)
                {
                    if (transform.position.x > _nextPosition.x)
                        _movement.x = -1;
                    else
                        _movement.x = 1;
                }
                else
                {
                    if (transform.position.y > _nextPosition.y)
                        _movement.y = -1;
                    else
                        _movement.y = 1;
                }
            }
            else
            {
                if (_currentPath.Count == 0)
                {
                    _isMovingToTarget = false;
                    _movement = Vector2Int.zero;
                    Debug.Log("Reached target!");
                    ClearPathDebugSprites();
                }
                else
                {
                    _nextPosition = _map.GetWorldPositionFromNormalizedPosition(_currentPath.Pop());
                }
            }
        }

        UpdateAnimator();
    }

    private void UpdateTarget(Vector3? targetPosition = null)
    {
        _targetPosition = targetPosition.HasValue ? targetPosition.Value : SearchNextTarget();

        var targetNormalizedCellPosition = _map.GetNormalizedCellPositionFromWorldPosition(_targetPosition);
        _currentPath = ComputePath(targetNormalizedCellPosition);
        _nextPosition = _map.GetWorldPositionFromNormalizedPosition(_currentPath.Pop());

        ClearPathDebugSprites();

        foreach (var step in _currentPath)
        {
            var pathStep = new GameObject("AIPathStep");
            pathStep.transform.position = _map.GetWorldPositionFromNormalizedPosition(step);
            pathStep.transform.localScale *= 5f;
            var nextPositionDebugSpriteRenderer = pathStep.AddComponent<SpriteRenderer>();
            nextPositionDebugSpriteRenderer.sprite = _debugSprite;
            //nextPositionDebugSpriteRenderer.color = Color.yellow;
            nextPositionDebugSpriteRenderer.sortingLayerName = "Player";

            _pathDebugSprites.Add(pathStep);
        }

        _isMovingToTarget = true;
    }

    private void ClearPathDebugSprites()
    {
        // Next position debug sprite
        foreach (var sprite in _pathDebugSprites)
            Destroy(sprite);

        _pathDebugSprites.Clear();
    }

    private void UpdateAnimator()
    {
        bool isMoving = _movement.sqrMagnitude > 0;

        _animator.SetFloat("Horizontal", _movement.x);
        _animator.SetFloat("Vertical", _movement.y);

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

        return _map.CellToWorld(randomCellPosition);
    }

    private Stack<Vector2Int> ComputePath(Vector2Int target)
    {
        var origin = _map.GetNormalizedCellPositionFromWorldPosition(transform.position);
        int[,] costMatrix = ComputeCostMatrix(origin);

        var path = new Stack<Vector2Int>();
        path.Push(target);

        var direction = Direction.None;
        while (origin != target && path.Count < 10000)
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
                path.Push(target);
        }

        return path;
    }

    private Dictionary<Direction, Vector2Int> GetNeighbours(Vector2Int position)
    {
        var neighbours = new Dictionary<Direction, Vector2Int>();

        var topPosition = new Vector2Int(position.x, position.y - 1);
        if (!_map.IsNormalizedCellPositionOutOfBound(topPosition))
            neighbours.Add(Direction.Up, topPosition);

        var bottomPosition = new Vector2Int(position.x, position.y + 1);
        if (!_map.IsNormalizedCellPositionOutOfBound(bottomPosition))
            neighbours.Add(Direction.Down, bottomPosition);

        var rightPosition = new Vector2Int(position.x + 1, position.y);
        if (!_map.IsNormalizedCellPositionOutOfBound(rightPosition))
            neighbours.Add(Direction.Right, rightPosition);

        var leftPosition = new Vector2Int(position.x - 1, position.y);
        if (!_map.IsNormalizedCellPositionOutOfBound(leftPosition))
            neighbours.Add(Direction.Left, leftPosition);

        return neighbours;
    }

    public int[,] ComputeCostMatrix(Vector2Int targetNormalizedCellPosition)
    {
        var costMatrix = new int[_map.MapSize.x + 1, _map.MapSize.y + 1];

        // We put all cells at the "infinity" value
        for (int x = 0; x <= _map.MapSize.x; x++)
            for (int y = 0; y <= _map.MapSize.y; y++)
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
                if (currentPosition.x + 1 <= _map.MapSize.x && IsAccessible(new Vector2Int(currentPosition.x + 1, currentPosition.y)) &&
                    costMatrix[currentPosition.x + 1, currentPosition.y] > id)
                {
                    costMatrix[currentPosition.x + 1, currentPosition.y] = id;
                    queue.Enqueue(new Vector2Int(currentPosition.x + 1, currentPosition.y));
                }
                // Bottom => TODO: Why don't <= with MapSize.y but yes for MapSize.x?
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
                string c = "_";

                if (costMatrix[x, _map.MapSize.y - y] >= 0 && costMatrix[x, _map.MapSize.y - y] < 26)
                    c = char.ConvertFromUtf32(97 + costMatrix[x, _map.MapSize.y - y]);
                else if (costMatrix[x, _map.MapSize.y - y] >= _map.MapSize.x * _map.MapSize.y)
                    c = "X";

                costMatrixString.Append(c + " ");
            }

            costMatrixString.Append("\n");
        }

        _costMatrixText.text = costMatrixString.ToString();
    }


    private bool IsAccessible(Vector2Int normalizedCellPosition)
    {
        return !_map.IsNormalizedCellPositionOutOfBound(normalizedCellPosition) &&
                _map.GetEntityType(normalizedCellPosition) != EEntityType.UnbreakableWall &&
                _map.GetEntityType(normalizedCellPosition) != EEntityType.DestructibleWall &&
                _map.GetEntityType(normalizedCellPosition) != EEntityType.Explosion &&
                _map.GetEntityType(normalizedCellPosition) != EEntityType.Bomb;
    }

    private void FixedUpdate()
    {
        float speed = (_gameSettings.PlayerBaseSpeed + (_player.SpeedBonus * _gameSettings.SpeedBonusIncrement)) * _debugSpeedFactor;
        _rigidbody.MovePosition(_rigidbody.position + _movement * speed * Time.fixedDeltaTime);
    }
}
