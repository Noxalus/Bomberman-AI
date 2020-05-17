using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ExplosionEvent : UnityEvent<Explosion> { }

public class Explosion : MonoBehaviour
{
    #region Events

    public ExplosionEvent OnExplosionFinished;

    #endregion

    #region Constants

    // Animator key constants
    private const string ANIMATOR_LEFT_SIDE_KEY = "Left";
    private const string ANIMATOR_RIGHT_SIDE_KEY = "Right";
    private const string ANIMATOR_TOP_SIDE_KEY = "Top";
    private const string ANIMATOR_BOTTOM_SIDE_KEY = "Bottom";
    private const string ANIMATOR_IS_BOUND_KEY = "IsBound";

    #endregion

    [Header("Assets references")]

    [SerializeField] ExplosionSprite _explosionCenter = null;

    public List<Vector2Int> ImpactedCells => _impactedCells;

    private Map _map = null;
    private Bomb _bomb = null;
    private List<Vector2Int> _impactedCells = new List<Vector2Int>();

    public void Initialize(Bomb bomb, Map map)
    {
        _bomb = bomb;
        _map = map;

        _impactedCells = new List<Vector2Int>();

        int power = bomb.Power;
        Vector2Int currentCellPosition = map.CellPosition(transform.position);

        var centerExplosion = Instantiate(_explosionCenter, transform);
        centerExplosion.transform.localPosition = Vector2.zero;
        centerExplosion.OnExplosionFinished += OnExplosionAnimationFinished;

        _impactedCells.Add(currentCellPosition);

        bool stopTop = false;
        bool stopRight = false;
        bool stopBottom = false;
        bool stopLeft = false;

        for (int i = 1; i <= power; i++)
        {
            if (!stopTop)
            {
                Vector2Int topPosition = new Vector2Int(0, i);
                stopTop = InstantiateExplosion(currentCellPosition, topPosition, i == power);
            }

            if (!stopBottom)
            {
                Vector2Int bottomPosition = new Vector2Int(0, -i);
                stopBottom = InstantiateExplosion(currentCellPosition, bottomPosition, i == power);
            }

            if (!stopLeft)
            {
                Vector2Int leftPosition = new Vector2Int(-i, 0);
                stopLeft = InstantiateExplosion(currentCellPosition, leftPosition, i == power);
            }

            if (!stopRight)
            {
                Vector2Int rightPosition = new Vector2Int(i, 0);
                stopRight = InstantiateExplosion(currentCellPosition, rightPosition, i == power);
            }
        }
    }

    private bool InstantiateExplosion(Vector2Int position, Vector2Int offset, bool isBound)
    {
        bool stop = false;

        EEntityType entityType = _map.GetEntityType(position + offset);

        if (entityType != EEntityType.UnbreakableWall || entityType == EEntityType.Explosion)
        {
            if (entityType == EEntityType.DestructibleWall)
            {
                stop = true;
            }

            var explosion = Instantiate(_explosionCenter, transform);
            var animator = explosion.GetComponent<Animator>();
            animator.SetBool(OffsetToSide(offset), true);
            animator.SetBool(ANIMATOR_IS_BOUND_KEY, isBound || stop);
            explosion.transform.localPosition = new Vector3(offset.x, offset.y, 0);

            _impactedCells.Add(position + offset);
        }
        else
        {
            stop = true;
        }

        return stop;
    }

    private string OffsetToSide(Vector2Int offset)
    {
        if (offset.x > 0)
        {
            return ANIMATOR_RIGHT_SIDE_KEY;
        }
        else if (offset.x < 0)
        {
            return ANIMATOR_LEFT_SIDE_KEY;
        }
        else if (offset.y > 0)
        {
            return ANIMATOR_TOP_SIDE_KEY;
        }
        else if (offset.y < 0)
        {
            return ANIMATOR_BOTTOM_SIDE_KEY;
        }

        throw new Exception($"No side for this offset: {offset.ToString()}");
    }

    private void OnExplosionAnimationFinished()
    {
        OnExplosionFinished?.Invoke(this);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var killer = _bomb.Player;

        if (collision.tag == "Player")
        {
            var player = collision.GetComponent<Player>();

            if (!player.IsInvincible && !player.IsDead)
            {
                if (killer != null)
                {
                    killer.OnKilledPlayer(player);
                }

                player.Kill(killer);
            }
        }
        else if (collision.tag == "DestructibleWall")
        {
            var wall = collision.GetComponent<DestructibleWall>();

            if (killer != null)
            {
                killer.OnDestroyedWall(wall);
            }

            wall.Explode();
        }
        else if (collision.tag == "Bonus")
        {
            var bonus = collision.GetComponent<Bonus>();

            if (killer != null)
            {
                killer.OnDestroyedBonus(bonus);
            }
            
            bonus.Explode();
        }
        else if (collision.tag == "Bomb")
        {
            collision.GetComponent<Bomb>().Explode();
        }
    }
}
