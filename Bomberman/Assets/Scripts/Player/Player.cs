using UnityEngine;

public class Player : MonoBehaviour
{
    private GameManager _gameManager = null;

    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _gameManager.AddBomb(transform.position);
        }
    }
}
