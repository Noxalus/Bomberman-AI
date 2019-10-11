using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Scene reference")]

    [SerializeField] private WallGenerator _wallGenerator = null;
    [SerializeField] private UIManager _uiManager = null;

    [Header("Assets reference")]

    [SerializeField] private Player _playerPrefab = null;
    [SerializeField] private GameSettings _gameSettings = null;

    public Map Map => _map;

    public List<Player> Players => _players;

    private Map _map = null;
    private List<Player> _players = new List<Player>();
    private List<Bomb> _bombs = new List<Bomb>();

    private void Start()
    {
        StartCoroutine(LoadMapScene());
    }

    IEnumerator LoadMapScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Map1", LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        OnSceneLoaded();
    }

    private void OnSceneLoaded()
    {
        _map = FindObjectOfType<Map>();

        if (_map != null)
        {
            _wallGenerator.Initialize(_map);
            _wallGenerator.GenerateWalls(_gameSettings.WallDensity);

            StartGame(4);
        }
    }

    private void StartGame(int playerCount = 1)
    {
        for (int i = 0; i < playerCount; i++)
        {
            var player = Instantiate(_playerPrefab, _map.GetSpawnPosition(i), Quaternion.identity);
            player.Initialize(i, _gameSettings.PlayersColor[i], this);
            _players.Add(player);
        }

        _uiManager.Initialize(_players);
    }

    public void AddBomb(Bomb bomb, Vector3 worldPosition)
    {
        var cellPosition = _map.GameGrid.WorldToCell(worldPosition);
        bomb.transform.position = cellPosition + _map.GameTilemap.tileAnchor;

        _bombs.Add(bomb);
    }
}
