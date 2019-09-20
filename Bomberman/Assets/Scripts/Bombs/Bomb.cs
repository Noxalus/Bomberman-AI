using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private Collider2D _collider = null;
    [SerializeField] private Animator _animator = null;

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
                _animator.SetTrigger("Explode");
            }
        }
    }
}
