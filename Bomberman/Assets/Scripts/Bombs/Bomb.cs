﻿using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Inner references")]

    [SerializeField] private Collider2D _collider = null;
    [SerializeField] private Animator _animator = null;

    [Header("Assets references")]

    [SerializeField] private Explosion _explosionPrefab = null;

    private Player _player;
    private Map _map;
    private float _timer;
    private int _power;

    private float _currentTimer;
    private bool _isExploding = false;

    private void Awake()
    {
        _collider.enabled = false;
    }

    public void Initialize(Player player, Map map, float timer, int power)
    {
        _player = player;
        _map = map;
        _timer = timer;
        _power = power;

        _currentTimer = _timer;
        _isExploding = false;
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

    public void Explode()
    {
        if (_isExploding)
            return;

        _isExploding = true;
        Explosion explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        explosion.Initialize(this, _map, _power);
        Destroy(gameObject);
    }
}
