using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Assets references")]

    [SerializeField] ExplosionPart _explosionCenter = null;
    [SerializeField] ExplosionPart _explosionUp = null;
    [SerializeField] ExplosionPart _explosionDown = null;
    [SerializeField] ExplosionPart _explosionLeft = null;
    [SerializeField] ExplosionPart _explosionRight = null;
    [SerializeField] ExplosionPart _explosionVertical = null;
    [SerializeField] ExplosionPart _explosionHorizontal = null;

    private Bomb _bomb = null;

    public void Initialize(Bomb bomb, int power)
    {
        _bomb = bomb;
        Vector2 cellSize = Vector2.one;
        var centerExplosion = Instantiate(_explosionCenter, transform);
        centerExplosion.transform.localPosition = Vector2.zero;

        for (int i = 1; i <= power; i++)
        {
            Vector2 topPosition = new Vector2(0, i * cellSize.y);
            var topExplosion = Instantiate((i == power) ? _explosionUp : _explosionVertical, transform);
            topExplosion.transform.localPosition = topPosition;

            Vector2 bottomPosition = new Vector2(0, -i * cellSize.y);
            var bottomExplosion = Instantiate((i == power) ? _explosionDown : _explosionVertical, transform);
            bottomExplosion.transform.localPosition = bottomPosition;

            Vector2 leftPosition = new Vector2(-i * cellSize.x, 0);
            var leftExplosion = Instantiate((i == power) ? _explosionLeft : _explosionHorizontal, transform);
            leftExplosion.transform.localPosition = leftPosition;

            Vector2 rightPosition = new Vector2(i * cellSize.x, 0);
            var rightExplosion = Instantiate((i == power) ? _explosionRight : _explosionHorizontal, transform);
            rightExplosion.transform.localPosition = rightPosition;
        }
    }
}
