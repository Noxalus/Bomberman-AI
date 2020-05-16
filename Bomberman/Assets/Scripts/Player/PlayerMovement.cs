using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerMovement : MonoBehaviour
{
    [Header("Inner references")]

    [SerializeField] private Player _player = null;
    [SerializeField] private Rigidbody2D _rigidbody = null;
    [SerializeField] private Animator _animator = null;
    [SerializeField] private PlayerInput _input = null;

    [Header("Assets")]

    [SerializeField] private GameSettings _gameSettings = null;

    private Vector2 _movement = Vector2.zero;

    private void OnEnable()
    {
        if (_input)
        {
            _input.actions.Enable();
        }
    }

    private void OnDisable()
    {
        if (_input)
        {
            _input.actions.Disable();
        }
    }

    private void Awake()
    {
        if (_input)
        {
            _input.actions.FindAction("Bomb").performed += OnBombKeyPerformed;
            _input.actions.FindAction("Move").performed += OnInputMovePerformed;
            _input.actions.FindAction("Move").canceled += OnInputMoveCanceled;
        }

        _player.OnSpawn.AddListener(OnPlayerSpawn);
        _player.OnKill.AddListener(OnPlayerKill);
    }

    private void OnDestroy()
    {
        if (_input)
        {
            _input.actions.FindAction("Bomb").performed -= OnBombKeyPerformed;
        }

        _player.OnSpawn.RemoveListener(OnPlayerSpawn);
        _player.OnKill.RemoveListener(OnPlayerKill);
    }

    private void OnPlayerSpawn(Player player)
    {
        if (_input)
        {
            _input.actions.Enable();
        }
    }

    private void OnPlayerKill(Player player)
    {
        if (_input)
        {
            _input.actions.Disable();
        }
    }

    #region Used for ML agents

    public void PlantBomb()
    {
        _player.AddBomb();
    }

    public void Move(Vector2 movement)
    {
        _movement = movement;
    }

    #endregion

    private void OnBombKeyPerformed(CallbackContext context)
    {
        _player.AddBomb();
    }

    private void OnInputMovePerformed(CallbackContext context)
    {
        _movement = context.ReadValue<Vector2>();
    }

    private void OnInputMoveCanceled(CallbackContext context)
    {
        _movement = Vector2.zero;
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
        float speed = _gameSettings.PlayerBaseSpeed + (_player.SpeedBonus * _gameSettings.SpeedBonusIncrement);
        _rigidbody.MovePosition(_rigidbody.position + _movement * speed * Time.fixedDeltaTime);
    }
}
