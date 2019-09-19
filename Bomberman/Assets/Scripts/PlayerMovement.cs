using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Rigidbody2D _rigidbody = null;
    [SerializeField] private Animator _animator = null;
    [SerializeField] private Grid _gameGrid = null;

    private Vector2 _movement = Vector2.zero;
    private Vector3Int _currentCell = Vector3Int.zero;
    private Vector3Int _previousCell = Vector3Int.zero;

    void Update()
    {
        _movement.x = Input.GetAxisRaw("Horizontal");
        _movement.y = Input.GetAxisRaw("Vertical");

        _animator.SetFloat("Horizontal", _movement.x);
        _animator.SetFloat("Vertical", _movement.y);
        _animator.SetFloat("Speed", _movement.sqrMagnitude);

        _currentCell = _gameGrid.WorldToCell(transform.position);

        if (_currentCell != _previousCell)
        {
            Debug.Log($"Current cell: {_currentCell.ToString()}");
        }

        _previousCell = _currentCell;
    }

    void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _movement * _moveSpeed * Time.fixedDeltaTime);
    }
}
