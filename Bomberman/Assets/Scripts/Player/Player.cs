using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PlayerDataChangeEvent : UnityEvent<Player> {}

public class Player : MonoBehaviour
{
    [Header("Events")]

    public PlayerDataChangeEvent OnScoreChange;
    public PlayerDataChangeEvent OnPowerChange;
    public PlayerDataChangeEvent OnBombCountChange;
    public PlayerDataChangeEvent OnSpeedChange;
    public PlayerDataChangeEvent OnKill; // When he just get killed
    public PlayerDataChangeEvent OnDeath; // When the death animation is finished

    [Header("Configuration")]

    [SerializeField] private int _maxBombCount = 1;
    [SerializeField] private int _bombPower = 1;
    [SerializeField] private float _bombTimer = 2f;
    [SerializeField] private int _speedBonus = 1;

    [Header("Inner references")]

    [SerializeField] private SpriteRenderer _spriteRenderer = null;
    [SerializeField] private Animator _animator = null;
    [SerializeField] private Rigidbody2D _rigidbody = null;

    [Header("Assets")]

    [SerializeField] private Bomb _bombPrefab = null;
    [SerializeField] private GameSettings _gameSettings = null;

    private GameManager _gameManager = null;
    private int _id = 0;
    private Color _color = Color.white;
    private int _score = 0;
    private int _currentBombCount = 1;
    private bool _isDead = false;

    public int Id => _id;
    public Color Color => _color;
    public int Score => _score;
    public int Power => _bombPower;
    public int BombCount => _currentBombCount;
    public int SpeedBonus => _speedBonus;

    private void Destroy()
    {
        //Destroy(gameObject);
    }

    public void Initialize(int id, Color color, GameManager gameManager)
    {
        _id = id;
        _color = color;
        _gameManager = gameManager;
        _spriteRenderer.color = color;
    }

    public void Respawn(Vector3 position)
    {
        _rigidbody.simulated = true;

        transform.position = position;

        // Default stats
        _maxBombCount = _gameSettings.PlayerBaseBombCount;
        _speedBonus = _gameSettings.PlayerBaseSpeedBonus;
        _bombPower = _gameSettings.PlayerBaseBombPower;

        _currentBombCount = _maxBombCount; 
        _isDead = false;
    }

    public void AddBomb()
    {
        if (_currentBombCount > 0)
        {
            var bomb = Instantiate(_bombPrefab, Vector2.zero, Quaternion.identity);
            bomb.Initialize(this, _gameManager.Map, _bombTimer, _bombPower);
            bomb.OnExplosion += OnBombExplosion;
            _currentBombCount--;

            _gameManager.AddBomb(bomb, transform.position);
            OnBombCountChange?.Invoke(this);
        }
    }

    public void UpdateBombCount(int amount)
    {
        _maxBombCount += amount;
        _currentBombCount += amount;
        OnBombCountChange?.Invoke(this);
    }

    public void UpdateSpeedBonus(int amount)
    {
        _speedBonus += amount;
        OnSpeedChange?.Invoke(this);
    }

    public void UpdatePower(int amount)
    {
        _bombPower += amount;
        OnPowerChange?.Invoke(this);
    }

    public void UpdateScore(int value)
    {
        _score += value;
        OnScoreChange?.Invoke(this);
    }

    public void OnBombExplosion()
    {
        _currentBombCount++;
        OnBombCountChange?.Invoke(this);
    }

    public void Kill(Player killer)
    {
        if (_isDead)
            return;

        _isDead = true;

        // Suicide?
        if (killer.Id == Id)
        {
            UpdateScore(-2);
        }

        Debug.Log($"Player killed by {killer.Id}");

        SoundManager.Instance.PlaySound("PlayerDeath");

        _animator.SetBool("IsDead", _isDead);

        OnKill?.Invoke(this);
    }

    public void OnDeathAnimationFinish()
    {
        OnDeath?.Invoke(this);
        _rigidbody.simulated = false;
    }
}
