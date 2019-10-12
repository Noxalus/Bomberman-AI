using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    [SerializeField] private Animator _animator = null;
    [SerializeField] private GameSettings _gameSettings = null;
    [SerializeField] private Bonus _bonusPrefab = null;

    private bool _isExploding = false;

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

    internal void Explode()
    {
        if (_isExploding)
            return;

        _isExploding = true;
        _animator.SetTrigger("Explode");
    }
}
