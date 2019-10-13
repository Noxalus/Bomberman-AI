using System;
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

    private Map _map = null;
    private Bomb _bomb = null;

    public void Initialize(Bomb bomb, Map map)
    {
        _bomb = bomb;
        _map = map;

        int power = bomb.Power;
        Vector3Int currentCellPosition = map.GameGrid.WorldToCell(transform.position);
        Vector2Int cellSize = Vector2Int.one;

        var centerExplosion = Instantiate(_explosionCenter, transform);
        centerExplosion.transform.localPosition = Vector2.zero;
        centerExplosion.OnExplosionFinished += OnExplosionFinished;

        bool stopTop = false;
        bool stopRight = false;
        bool stopBottom = false;
        bool stopLeft = false;

        for (int i = 1; i <= power; i++)
        {
            if (!stopTop)
            {
                Vector3Int topPosition = new Vector3Int(0, i * cellSize.y, 0);
                stopTop = InstantiateExplosion(currentCellPosition, topPosition, i == power);
            }

            if (!stopBottom)
            {
                Vector3Int bottomPosition = new Vector3Int(0, -i * cellSize.y, 0);
                stopBottom = InstantiateExplosion(currentCellPosition, bottomPosition, i == power);
            }

            if (!stopLeft)
            {
                Vector3Int leftPosition = new Vector3Int(-i * cellSize.x, 0, 0);
                stopLeft = InstantiateExplosion(currentCellPosition, leftPosition, i == power);
            }

            if (!stopRight)
            {
                Vector3Int rightPosition = new Vector3Int(i * cellSize.x, 0, 0);
                stopRight = InstantiateExplosion(currentCellPosition, rightPosition, i == power);
            }
        }
    }

    private bool InstantiateExplosion(Vector3Int position, Vector3Int offset, bool isBound)
    {
        bool stop = false;

        EEntityType entityType = _map.GetEntityType(
            _map.GameGrid.CellToWorld(position + offset)
        );

        if (entityType != EEntityType.UnbreakableWall)
        {
            if (entityType == EEntityType.DestructibleWall)
            {
                stop = true;
            }

            var topExplosion = Instantiate(_explosionCenter, transform);
            var topExplosionAnimator = topExplosion.GetComponent<Animator>();
            topExplosionAnimator.SetBool(OffsetToSide(offset), true);
            topExplosionAnimator.SetBool(ANIMATOR_IS_BOUND_KEY, isBound || stop);
            topExplosion.transform.localPosition = offset;
        }
        else
        {
            stop = true;
        }

        return stop;
    }

    private string OffsetToSide(Vector3Int offset)
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
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            var killer = _bomb.Player;
            collision.GetComponent<Player>().Kill(killer);
            killer.UpdateScore(1);
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
