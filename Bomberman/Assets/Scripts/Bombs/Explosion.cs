using System;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Assets references")]

    [SerializeField] ExplosionSprite _explosionCenter = null;

    private Bomb _bomb = null;

    public void Initialize(Bomb bomb, Map map, int power)
    {
        _bomb = bomb;
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
                EEntityType topEntityType = map.GetEntityType(
                    map.GameGrid.CellToWorld(currentCellPosition + topPosition) + map.GameTilemap.tileAnchor
                );

                if (topEntityType != EEntityType.UnbreakableWall)
                {
                    var topExplosion = Instantiate(_explosionCenter, transform);
                    var topExplosionAnimator = topExplosion.GetComponent<Animator>();
                    topExplosionAnimator.SetBool("Top", true);
                    topExplosionAnimator.SetBool("Bound", (i == power));
                    topExplosion.transform.localPosition = topPosition;

                    if (topEntityType == EEntityType.DestructibleWall)
                    {
                        stopTop = true;
                    }
                }
                else
                {
                    stopTop = true;
                }
            }

            if (!stopBottom)
            {
                Vector3Int bottomPosition = new Vector3Int(0, -i * cellSize.y, 0);
                EEntityType bottomEntityType = map.GetEntityType(
                    map.GameGrid.CellToWorld(currentCellPosition + bottomPosition) + map.GameTilemap.tileAnchor
                );

                if (bottomEntityType != EEntityType.UnbreakableWall)
                {
                    var bottomExplosion = Instantiate(_explosionCenter, transform);
                    var bottomExplosionAnimator = bottomExplosion.GetComponent<Animator>();
                    bottomExplosionAnimator.SetBool("Bottom", true);
                    bottomExplosionAnimator.SetBool("Bound", (i == power));
                    bottomExplosion.transform.localPosition = bottomPosition;

                    if (bottomEntityType == EEntityType.DestructibleWall)
                    {
                        stopBottom = true;
                    }
                }
                else
                {
                    stopBottom = true;
                }
            }

            if (!stopLeft)
            {
                Vector3Int leftPosition = new Vector3Int(-i * cellSize.x, 0, 0);
                EEntityType leftEntityType = map.GetEntityType(
                    map.GameGrid.CellToWorld(currentCellPosition + leftPosition) + map.GameTilemap.tileAnchor
                );

                if (leftEntityType != EEntityType.UnbreakableWall)
                {
                    var leftExplosion = Instantiate(_explosionCenter, transform);
                    var leftExplosionAnimator = leftExplosion.GetComponent<Animator>();
                    leftExplosionAnimator.SetBool("Left", true);
                    leftExplosionAnimator.SetBool("Bound", (i == power));
                    leftExplosion.transform.localPosition = leftPosition;

                    if (leftEntityType == EEntityType.DestructibleWall)
                    {
                        stopLeft = true;
                    }
                }
                else
                {
                    stopLeft = true;
                }
            }


            if (!stopRight)
            {
                Vector3Int rightPosition = new Vector3Int(i * cellSize.x, 0, 0);
                EEntityType rightEntityType = map.GetEntityType(
                    map.GameGrid.CellToWorld(currentCellPosition + rightPosition) + map.GameTilemap.tileAnchor
                );

                if (rightEntityType != EEntityType.UnbreakableWall)
                {
                    var rightExplosion = Instantiate(_explosionCenter, transform);
                    var rightExplosionAnimator = rightExplosion.GetComponent<Animator>();
                    rightExplosionAnimator.SetBool("Right", true);
                    rightExplosionAnimator.SetBool("Bound", (i == power));
                    rightExplosion.transform.localPosition = rightPosition;

                    if (rightEntityType == EEntityType.DestructibleWall)
                    {
                        stopRight = true;
                    }
                }
                else
                {
                    stopRight = true;
                }
            }
        }
    }

    private void OnExplosionFinished()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<Player>().Kill();
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
