using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIBehaviour : MonoBehaviour
{
    #region Events

    public PlayerEvent OnCostMapChanged;
    public PlayerEvent OnPathChanged;
    public PlayerEvent OnTargetReached;

    #endregion

    #region Serialized fields

    [Header("Inner references")]

    [SerializeField] private Player _player = null;
    [SerializeField] private Rigidbody2D _rigidbody = null;
    [SerializeField] private Animator _animator = null;

    [Header("Assets")]

    [SerializeField] private GameSettings _gameSettings = null;

    [Header("Debug")]

    [SerializeField] float _debugSpeedFactor = 0.25f;

    #endregion

    #region Private fields

    private Vector2 _movement = Vector2.zero;
    private Vector3 _targetPosition = Vector3.zero;
    private bool _isMovingToTarget = false;
    private bool _isEscapingDanger = false;
    private Stack<Vector2Int> _currentPath = new Stack<Vector2Int>();
    private Vector3 _nextPosition = Vector3.zero;
    private AIManager _aiManager;
    private float _speed;
    private bool _isEnabled = true;

    #endregion

    #region Properties

    public Stack<Vector2Int> CurrentPath => _currentPath;

    #endregion

    public void Enable(bool enable = true)
    {
        _isEnabled = enable;
        Clear();
    }

    public void Initialize(AIManager aiManager)
    {
        Clear();

        _aiManager = aiManager;
        _isEnabled = true;
    }

    public void Clear()
    {
        _movement = Vector2.zero;
        _targetPosition = Vector3.zero;
        _isMovingToTarget = false;
        _isEscapingDanger = false;
        _nextPosition = Vector3.zero;
        _currentPath.Clear();
    }

    private Vector2Int CellPosition()
    {
        return _aiManager.CellPosition(transform.position);
    }

    private void Update()
    {
        if (!_isEnabled)
            return;

        // Debug: Place a target position for the AI
        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Input.mousePosition;
            var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            var cellPosition = _aiManager.CellPosition(worldPosition);

            if (_aiManager.IsAccessible(cellPosition))
            {
                worldPosition = _aiManager.WorldPosition(cellPosition);
                UpdateTarget(worldPosition);
            }
            else
            {
                Debug.LogWarning("Position not accessible for the bot...");
            }
        }

        UpdateMovement();
        UpdateAnimator();
    }

    private void UpdateMovement()
    {
        _movement = Vector2Int.zero;

        if (!_isEscapingDanger && IsInDanger())
        {
            Vector2Int? safePosition = _aiManager.FindNearestSafeCell(CellPosition());

            if (safePosition.HasValue)
            {
                UpdateTarget(_aiManager.WorldPosition(safePosition.Value));
                _isEscapingDanger = true;
            }
            else
                Debug.Log("AI is doomed, just waiting to die...");

            return;
        }

        if (_isMovingToTarget)
        {
            if (!_aiManager.IsAccessible(_aiManager.CellPosition(_nextPosition)))
            {
                UpdateTarget(_targetPosition);
                return;
            }

            if (MoveToTarget())
            {
                if (_currentPath.Count == 0)
                {
                    _isMovingToTarget = false;
                    _movement = Vector2Int.zero;
                    Debug.Log("Reached target!");

                    if (_isEscapingDanger)
                        _isEscapingDanger = false;

                    if (CanPlantBomb())
                        _player.OnPlantBomb.Invoke(_player);
                    
                    _rigidbody.position = _targetPosition;

                    OnTargetReached?.Invoke(_player);
                }
                else
                {
                    Debug.Log("Reached next position");

                    //if (CanPlantBomb())
                    //    _player.OnPlantBomb.Invoke(_player);

                    _rigidbody.position = _nextPosition;
                    _nextPosition = _aiManager.WorldPosition(_currentPath.Pop());
                    MoveToTarget();
                }
            }
        }
        else
        {
            var newTarget = _aiManager.GetBestGoalPosition(_aiManager.CellPosition(transform.position));

            if (newTarget.HasValue)
            {
                UpdateTarget(_aiManager.WorldPosition(newTarget.Value));
            }
            else
            {
                Debug.LogWarning("No interesting goal for the AI!");
            }
        }
    }

    private bool MoveToTarget()
    {
        var distance = new Vector2(
            Mathf.Abs(transform.position.x - _nextPosition.x),
            Mathf.Abs(transform.position.y - _nextPosition.y)
        );

        if (distance.x > 2 * (_speed * Time.fixedDeltaTime) || distance.y > 2 * (_speed * Time.fixedDeltaTime))
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

            return false;
        }
        else
        {
            return true;
        }
    }

    private void UpdateTarget(Vector3? targetPosition = null)
    {
        _targetPosition = targetPosition.HasValue ? targetPosition.Value : SearchNextTarget();

        var targetCellPosition = _aiManager.CellPosition(_targetPosition);
        var origin = _aiManager.CellPosition(transform.position);
        _currentPath = _aiManager.ComputePath(origin, targetCellPosition);

        OnCostMapChanged?.Invoke(_player);

        if (_currentPath.Count == 0)
        {
            Debug.LogWarning("No way to reach the target !");
            return;
        }

        _nextPosition = _aiManager.WorldPosition(_currentPath.Pop());
        _isMovingToTarget = true;

        OnPathChanged?.Invoke(_player);
    }

    private void UpdateAnimator()
    {
        bool isMoving = _movement.sqrMagnitude > 0;
        float animationSpeed = _speed / (_gameSettings.PlayerBaseSpeed + (_gameSettings.PlayerBaseSpeedBonus * _gameSettings.SpeedBonusIncrement));

        //_animator.SetFloat("AnimationSpeed", animationSpeed);
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
        for (int x = 0; x < _aiManager.AreaSize.x; x++)
        {
            for (int y = 0; y < _aiManager.AreaSize.y; y++)
            {
                var cellPosition = new Vector2Int(x, y);
                if (_aiManager.IsAccessible(cellPosition))
                {
                    availablePositions.Add(cellPosition);
                }
            }
        }

        var randomNormalizedCellPosition = availablePositions[Random.Range(0, availablePositions.Count)];
        return _aiManager.WorldPosition(randomNormalizedCellPosition);
    }

    private bool IsInDanger()
    {
        return _aiManager.GetDangerLevel(CellPosition()) > 0;
    }

    private bool CanPlantBomb()
    {
        if (_player.BombCount > 0)
        {
            // Can escape a potential bomb planting?
            short[,] dangerMatrix = _aiManager.SimulateBombPlanting(CellPosition(), _player.Power);
            Vector2Int? safePosition = _aiManager.FindNearestSafeCell(CellPosition(), dangerMatrix);

            return safePosition.HasValue;
        }

        return false;
    }

    private void FixedUpdate()
    {
        if (!_isEnabled)
            return;

        _speed = (_gameSettings.PlayerBaseSpeed + (_player.SpeedBonus * _gameSettings.SpeedBonusIncrement)) * _debugSpeedFactor;
        _rigidbody.MovePosition(_rigidbody.position + _movement * _speed * Time.fixedDeltaTime);
    }
}
