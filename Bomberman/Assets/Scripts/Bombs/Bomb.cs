using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class BombEvent : UnityEvent<Bomb> { }

public class Bomb : MonoBehaviour
{
    #region Events

    [Header("Events")]

    public BombEvent OnExplosion;
    public BombEvent OnWillExplodeSoon;

    #endregion

    #region Serialized fields

    [Header("Inner references")]

    [SerializeField] private Collider2D _collider = null;

    #endregion

    #region Private fields

    private Player _player;
    private Map _map;
    private float _timer;
    private int _power;
    private float _currentTimer;
    private bool _isExploding = false;
    // 0 = no danger, 1 = bomb planted, 2 = bomb will explode soon
    private short _dangerLevel = 0;

    #endregion

    #region Properties

    public Player Player => _player;
    public int Power => _power;
    public short DangerLevel => _dangerLevel;

    #endregion

    private void Awake()
    {
        _collider.enabled = false;
    }

    public void Initialize(float timer, int power, Map map)
    {
        _timer = timer;
        _power = power;
        _map = map;

        InitializeInternal();
    }

    public void Initialize(Player player, Map map)
    {
        _player = player;
        _timer = player.BombTimer;
        _power = player.Power;
        _map = map;

        InitializeInternal();
    }

    private void InitializeInternal()
    {
        _currentTimer = _timer;
        _isExploding = false;
        _dangerLevel = 1;
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

            if (_dangerLevel < 2 && _currentTimer < 1f)
            {
                _dangerLevel = 2;
                OnWillExplodeSoon?.Invoke(this);
            }

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

    public static List<Vector2Int> FindImpactedCells(Map map, Vector2Int position, int power)
    {
        List<Vector2Int> impactedCells = new List<Vector2Int>();

        impactedCells.Add(position);

        bool stopTop = false;
        bool stopRight = false;
        bool stopBottom = false;
        bool stopLeft = false;

        for (int i = 1; i <= power; i++)
        {
            if (!stopTop)
            {
                Vector2Int topPosition = position + new Vector2Int(0, i);

                if (map.IsAccessible(topPosition) || map.GetEntityType(topPosition) == EEntityType.Bomb)
                {
                    impactedCells.Add(topPosition);
                }
                else
                {
                    stopTop = true;
                }
            }

            if (!stopBottom)
            {
                Vector2Int bottomPosition = position + new Vector2Int(0, -i);

                if (map.IsAccessible(bottomPosition) || map.GetEntityType(bottomPosition) == EEntityType.Bomb)
                {
                    impactedCells.Add(bottomPosition);
                }
                else
                {
                    stopBottom = true;
                }
            }

            if (!stopLeft)
            {
                Vector2Int leftPosition = position + new Vector2Int(-i, 0);

                if (map.IsAccessible(leftPosition) || map.GetEntityType(leftPosition) == EEntityType.Bomb)
                {
                    impactedCells.Add(leftPosition);
                }
                else
                {
                    stopLeft = true;
                }
            }

            if (!stopRight)
            {
                Vector2Int rightPosition = position + new Vector2Int(i, 0);

                if (map.IsAccessible(rightPosition) || map.GetEntityType(rightPosition) == EEntityType.Bomb)
                {
                    impactedCells.Add(rightPosition);
                }
                else
                {
                    stopRight = true;
                }
            }
        }

        return impactedCells;
    }
}
