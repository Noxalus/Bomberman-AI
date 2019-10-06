using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    [SerializeField] private Animator _animator = null;

    private bool _isExploding = false;

    public void Destroy()
    {
        Destroy(gameObject);
    }

    internal void Explode()
    {
        if (_isExploding)
            return;

        _isExploding = true;
        _animator.SetTrigger("Explode");
    }
}
