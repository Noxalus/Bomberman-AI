using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player _player = null;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Rigidbody2D _rigidbody = null;
    [SerializeField] private Animator _animator = null;
    [SerializeField] private PlayerInput _input = null;

    private Vector2 _movement = Vector2.zero;

    private void Awake()
    {
        _input.actions.FindAction("Bomb").performed += ctx => _player.AddBomb();
        _input.actions.FindAction("Move").performed += OnInputMove;
        _input.actions.FindAction("Move").canceled += ctx => _movement = Vector2.zero;
    }

    private void OnEnable()
    {
        _input.actions.Enable();
    }

    private void OnDisable()
    {
        _input.actions.Disable();
    }

    private void OnInputMove(CallbackContext context)
    {
        _movement = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        _animator.SetFloat("Horizontal", _movement.x);
        _animator.SetFloat("Vertical", _movement.y);
        _animator.SetFloat("Speed", _movement.sqrMagnitude);
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _movement * _moveSpeed * Time.fixedDeltaTime);
    }
}
