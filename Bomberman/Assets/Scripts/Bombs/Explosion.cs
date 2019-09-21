using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Assets references")]

    [SerializeField] ExplosionPart _explosionCenter = null;

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
            var topExplosion = Instantiate(_explosionCenter, transform);
            var topExplosionAnimator = topExplosion.GetComponent<Animator>();
            topExplosionAnimator.SetBool("Top", true);
            topExplosionAnimator.SetBool("Bound", (i == power));
            topExplosion.transform.localPosition = topPosition;

            Vector2 bottomPosition = new Vector2(0, -i * cellSize.y);
            var bottomExplosion = Instantiate(_explosionCenter, transform);
            var bottomExplosionAnimator = bottomExplosion.GetComponent<Animator>();
            bottomExplosionAnimator.SetBool("Bottom", true);
            bottomExplosionAnimator.SetBool("Bound", (i == power));
            bottomExplosion.transform.localPosition = bottomPosition;

            Vector2 leftPosition = new Vector2(-i * cellSize.x, 0);
            var leftExplosion = Instantiate(_explosionCenter, transform);
            var leftExplosionAnimator = leftExplosion.GetComponent<Animator>();
            leftExplosionAnimator.SetBool("Left", true);
            leftExplosionAnimator.SetBool("Bound", (i == power));
            leftExplosion.transform.localPosition = leftPosition;

            Vector2 rightPosition = new Vector2(i * cellSize.x, 0);
            var rightExplosion = Instantiate(_explosionCenter, transform);
            var rightExplosionAnimator = rightExplosion.GetComponent<Animator>();
            rightExplosionAnimator.SetBool("Right", true);
            rightExplosionAnimator.SetBool("Bound", (i == power));
            rightExplosion.transform.localPosition = rightPosition;
        }
    }
}
