using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIBehaviour : MonoBehaviour
{
    [Header("Inner references")]

    [SerializeField] private Player _player = null;
    [SerializeField] private Rigidbody2D _rigidbody = null;
    [SerializeField] private Animator _animator = null;

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
    private AIManager _aiManager;

    // Debug
    private List<GameObject> _pathDebugSprites = new List<GameObject>();

    public void Initialize(AIManager aiManager)
    {
        _aiManager = aiManager;
    }

    private void Update()
    {
        // Debug: Place a target position for the AI
        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Input.mousePosition;
            var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            var normalizedCellPosition = _aiManager.CellPosition(worldPosition);

            if (_aiManager.IsAccessible(normalizedCellPosition))
            {
                worldPosition = _aiManager.WorldPosition(normalizedCellPosition);
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

        if (_isMovingToTarget)
        {
            if (MoveToTarget())
            {
                if (_currentPath.Count == 0)
                {
                    _isMovingToTarget = false;
                    _movement = Vector2Int.zero;
                    Debug.Log("Reached target!");
                    ClearPathDebugSprites();
                    _rigidbody.position = _targetPosition;
                }
                else
                {
                    Debug.Log("Reached next position");
                    _rigidbody.position = _nextPosition;
                    _nextPosition = _aiManager.WorldPosition(_currentPath.Pop());
                    MoveToTarget();
                }
            }
        }
    }

    private bool MoveToTarget()
    {
        float speed = (_gameSettings.PlayerBaseSpeed + (_player.SpeedBonus * _gameSettings.SpeedBonusIncrement)) * _debugSpeedFactor;

        var distance = new Vector2(
               Mathf.Abs(transform.position.x - _nextPosition.x),
               Mathf.Abs(transform.position.y - _nextPosition.y)
           );

        if (distance.x > speed * Time.fixedDeltaTime || distance.y > speed * Time.fixedDeltaTime)
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

            Debug.Log("Movement: " + _movement);

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

        if (_currentPath.Count == 0)
        {
            Debug.LogWarning("No way to reach the target !");
            return;
        }

        _nextPosition = _aiManager.WorldPosition(_currentPath.Pop());

        ClearPathDebugSprites();

        foreach (var step in _currentPath)
        {
            var pathStep = new GameObject("AIPathStep");
            pathStep.transform.position = _aiManager.WorldPosition(step);
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
        for (int x = 0; x < _aiManager.AreaSize.x; x++)
        {
            for (int y = 0; y < _aiManager.AreaSize.y; y++)
            {
                var normalizedCellPosition = new Vector2Int(x, y);
                if (_aiManager.IsAccessible(normalizedCellPosition))
                {
                    availablePositions.Add(normalizedCellPosition);
                }
            }
        }

        var randomNormalizedCellPosition = availablePositions[Random.Range(0, availablePositions.Count)];
        return _aiManager.WorldPosition(randomNormalizedCellPosition);
    }

    private void FixedUpdate()
    {
        float speed = (_gameSettings.PlayerBaseSpeed + (_player.SpeedBonus * _gameSettings.SpeedBonusIncrement)) * _debugSpeedFactor;
        _rigidbody.MovePosition(_rigidbody.position + _movement * speed * Time.fixedDeltaTime);
    }
}
