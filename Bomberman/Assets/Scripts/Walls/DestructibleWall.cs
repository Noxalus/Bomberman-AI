using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[System.Serializable]
public class DestructibleWallEvent : UnityEvent<DestructibleWall> {}

public class DestructibleWall : MonoBehaviour
{
    [Header("Events")]

    public DestructibleWallEvent OnExplode;

    [Header("Inner references")]

    [SerializeField] private Animator _animator = null;

    [Header("Assets")]

    [SerializeField] private GameSettings _gameSettings = null;
    [SerializeField] private Bonus _bonusPrefab = null;

    private bool _isExploding = false;

    // Call by the animator
    public void Destroy()
    {
        // Spanw a bonus?
        if (Random.value < _gameSettings.BonusProbability)
        {
            Bonus bonus = Instantiate(_bonusPrefab);
            bonus.Initalize(_gameSettings.GetAvailableBonus());
            bonus.transform.position = transform.position;
        }

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
