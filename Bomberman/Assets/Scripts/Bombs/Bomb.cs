using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Inner references")]

    [SerializeField] private Collider2D _collider = null;
    [SerializeField] private Animator _animator = null;

    [Header("Assets references")]

    [SerializeField] private Explosion _explosionPrefab = null;

    private Player _player;
    private float _timer;
    private int _power;

    private float _currentTimer;

    private void Awake()
    {
        _collider.enabled = false;
    }

    public void Initialize(Player player, float timer, int power)
    {
        _player = player;
        _timer = timer;
        _power = power;

        _currentTimer = _timer;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
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

    private void Explode()
    {
        Explosion explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        explosion.Initialize(this, _power);
        Destroy(gameObject);
    }
}
