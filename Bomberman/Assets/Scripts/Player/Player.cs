using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Bomb _bombPrefab = null;

    private int _id = 0;
    private GameManager _gameManager = null;
    private int _maxBombCount = 1;
    private int _currentBombCount = 1;
    private float _bombTimer = 2f;
    private int _bombPower = 1;

    public void Initialize(int id, GameManager gameManager)
    {
        _id = id;
        _gameManager = gameManager;
    }

    public void AddBomb()
    {
        var bomb = Instantiate(_bombPrefab, Vector2.zero, Quaternion.identity);
        bomb.Initialize(this, _gameManager.Map, _bombTimer, _bombPower);

        _gameManager.AddBomb(bomb, transform.position);

        _currentBombCount--;
    }

    public void Kill()
    {
        Debug.Log("Player killed");
        //Destroy(gameObject);
    }
}
