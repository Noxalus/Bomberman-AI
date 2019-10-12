using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player _player = null;
    [SerializeField] private float _baseSpeed = 5f;
    [SerializeField] private Rigidbody2D _rigidbody = null;
    [SerializeField] private Animator _animator = null;
    [SerializeField] private PlayerInput _input = null;
    [SerializeField] private GameSettings _gameSettings = null;

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

        bool isMoving = _movement.sqrMagnitude > 0;

        if (isMoving)
        {
            _animator.SetFloat("PreviousHorizontal", _movement.x);
            _animator.SetFloat("PreviousVertical", _movement.y);
        }

        _animator.SetBool("IsMoving", isMoving);
    }

    private void FixedUpdate()
    {
        float speed = _baseSpeed + (_player.SpeedBonus * _gameSettings.SpeedBonusIncrement);
        _rigidbody.MovePosition(_rigidbody.position + _movement * speed * Time.fixedDeltaTime);
    }
}
