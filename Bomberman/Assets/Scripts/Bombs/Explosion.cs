using System;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
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

    public List<ExplosionSprite> ExplosionSprites => _explosionSprites;

    private Map _map = null;
    private Bomb _bomb = null;
    private List<ExplosionSprite> _explosionSprites = new List<ExplosionSprite>();

    public void Initialize(Bomb bomb, Map map)
    {
        _bomb = bomb;
        _map = map;

        int power = bomb.Power;
        Vector2Int currentCellPosition = map.CellPosition(transform.position);
        Vector2Int cellSize = Vector2Int.one;

        var centerExplosion = Instantiate(_explosionCenter, transform);
        centerExplosion.transform.localPosition = Vector2.zero;
        centerExplosion.OnExplosionFinished += OnExplosionFinished;

        _map.SetEntityType(EEntityType.Explosion, centerExplosion.transform.position);
        _explosionSprites.Add(centerExplosion);

        bool stopTop = false;
        bool stopRight = false;
        bool stopBottom = false;
        bool stopLeft = false;

        for (int i = 1; i <= power; i++)
        {
            if (!stopTop)
            {
                Vector2Int topPosition = new Vector2Int(0, i * cellSize.y);
                stopTop = InstantiateExplosion(currentCellPosition, topPosition, i == power);
            }

            if (!stopBottom)
            {
                Vector2Int bottomPosition = new Vector2Int(0, -i * cellSize.y);
                stopBottom = InstantiateExplosion(currentCellPosition, bottomPosition, i == power);
            }

            if (!stopLeft)
            {
                Vector2Int leftPosition = new Vector2Int(-i * cellSize.x, 0);
                stopLeft = InstantiateExplosion(currentCellPosition, leftPosition, i == power);
            }

            if (!stopRight)
            {
                Vector2Int rightPosition = new Vector2Int(i * cellSize.x, 0);
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

            _map.SetEntityType(EEntityType.Explosion, explosion.transform.position);

            _explosionSprites.Add(explosion);
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

    private void OnExplosionFinished()
    {
        foreach (var explosionSprite in _explosionSprites)
        {
            if (_map.GetEntityType(explosionSprite.transform.position) != EEntityType.Bonus)
                _map.SetEntityType(EEntityType.None, explosionSprite.transform.position);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            var player = collision.GetComponent<Player>();

            if (!player.IsInvincible && !player.IsDead)
            {
                var killer = _bomb.Player;

                player.Kill(killer);
                killer.UpdateScore(1);
            }
        }
        else if (collision.tag == "DestructibleWall")
        {
            collision.GetComponent<DestructibleWall>().Explode();
        }
        else if (collision.tag == "Bomb")
        {
            collision.GetComponent<Bomb>().Explode();
        }
    }
}
