using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PlayerDataChangeEvent : UnityEvent<Player>
{
}

public class Player : MonoBehaviour
{
    public PlayerDataChangeEvent OnPowerChange;
    public PlayerDataChangeEvent OnBombCountChange;
    public PlayerDataChangeEvent OnSpeedChange;

    [SerializeField] private Bomb _bombPrefab = null;

    private int _id = 0;
    private GameManager _gameManager = null;
    private int _maxBombCount = 1;
    private int _currentBombCount = 1;
    private float _bombTimer = 2f;
    private int _bombPower = 1;
    private int _speedBonus = 1;

    public int Power => _bombPower;
    public int BombCount => _currentBombCount;
    public int SpeedBonus => _speedBonus;

    public int Id => _id;

    public void Initialize(int id, GameManager gameManager)
    {
        _id = id;
        _gameManager = gameManager;
    }

    public void AddBomb()
    {
        if (_currentBombCount > 0)
        {
            var bomb = Instantiate(_bombPrefab, Vector2.zero, Quaternion.identity);
            bomb.Initialize(this, _gameManager.Map, _bombTimer, _bombPower);
            bomb.OnExplosion += OnBombExplosion;
            _currentBombCount--;

            _gameManager.AddBomb(bomb, transform.position);
            OnBombCountChange?.Invoke(this);
        }
    }

    public void OnBombExplosion()
    {
        _currentBombCount++;
        OnBombCountChange?.Invoke(this);
    }

    public void Kill()
    {
        Debug.Log("Player killed");
        //Destroy(gameObject);
    }
}
