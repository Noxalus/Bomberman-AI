using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    [SerializeField] private Animator _animator = null;

    public void Destroy()
    {
        Destroy(gameObject);
    }

    internal void Explode()
    {
        _animator.SetTrigger("Explode");
    }
}
