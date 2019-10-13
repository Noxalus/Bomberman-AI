using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class BombEvent : UnityEvent<Bomb> { }

public class Bomb : MonoBehaviour
{
    [Header("Events")]

    public BombEvent OnExplosion;

    [Header("Inner references")]

    [SerializeField] private Collider2D _collider = null;

    public int Power => _power;

    private Player _player;
    private Map _map;
    private float _timer;
    private int _power;

    private float _currentTimer;
    private bool _isExploding = false;

    public Player Player => _player;

    private void Awake()
    {
        _collider.enabled = false;
    }

    public void Initialize(float timer, int power)
    {
        _timer = timer;
        _power = power;

        Initialize();
    }

    public void Initialize(Player player)
    {
        _player = player;
        _timer = player.BombTimer;
        _power = player.Power;

        Initialize();
    }

    private void Initialize()
    {
        _currentTimer = _timer;
        _isExploding = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            _collider.enabled = true;
        }
    }

    private void Update()
    {
        if (_currentTimer > 0)
        {
            _currentTimer -= Time.deltaTime;

            if (_currentTimer <= 0)
            {
                Explode();
            }
        }
    }

    public void Explode()
    {
        if (_isExploding)
            return;

        _isExploding = true;

        SoundManager.Instance.PlaySound("BombExplode");

        OnExplosion?.Invoke(this);

        Destroy(gameObject);
    }
}
