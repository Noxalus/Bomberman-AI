using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DestructibleWallEvent : UnityEvent<DestructibleWall> {}

public class DestructibleWall : MonoBehaviour
{
    [Header("Events")]

    public DestructibleWallEvent OnExplode;
    public DestructibleWallEvent OnDestroy;

    [Header("Inner references")]

    [SerializeField] private Animator _animator = null;

    private bool _isExploding = false;

    // Call by the animator
    public void Destroy()
    {
        OnDestroy?.Invoke(this);
        Destroy(gameObject);
    }

    public void Explode()
    {
        if (_isExploding)
            return;

        _isExploding = true;
        _animator.SetTrigger("Explode");

        OnExplode?.Invoke(this);
    }
}
