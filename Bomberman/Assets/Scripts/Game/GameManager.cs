using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Scene reference")]

    [SerializeField] private UIManager _uiManager = null;
    [SerializeField] private DebugManager _debugManager = null;

    [Header("Assets reference")]

    [SerializeField] private GameSettings _gameSettings = null;
    [SerializeField] private Player _playerPrefab = null;
    [SerializeField] private Bomb _bombPrefab = null;
    [SerializeField] private Explosion _explosionPrefab = null;

    public Map Map => _map;

    public List<Player> Players => _players;

    private Map _map = null;
    private List<Player> _players = new List<Player>();
    private List<Bomb> _bombs = new List<Bomb>();
    private int _deadPlayerCount = 0;

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
            _debugManager.Initialize(_map);
            StartGame(_gameSettings.PlayersCount);
        }
        else
        {
            throw new Exception("The loaded map doesn't contain a Map component!");
        }
    }

    private void StartGame(int playerCount = 1)
    {
        for (int i = 0; i < playerCount; i++)
        {
            var player = Instantiate(_playerPrefab);
            player.Initialize(i, _gameSettings.PlayersColor[i]);
            player.OnDeath.AddListener(OnPlayerDeath);
            player.OnPlantBomb.AddListener(AddBomb);
            _players.Add(player);
        }

        _uiManager.Initialize(_players);

        StartRound();
    }

    private void StartRound()
    {
        _map.Clear();
        ClearBombs();

        MusicManager.Instance.PlayMusic(_map.Music);
        _map.GenerateDestrucibleWalls(_gameSettings.WallDensity);
        _deadPlayerCount = 0;

        foreach (var player in _players)
        {
            player.Spawn(_map.GetSpawnPosition(player.Id));
        }
    }

    private void OnPlayerDeath(Player player)
    {
        player.OnDeath.RemoveListener(OnPlayerDeath);

        _deadPlayerCount++;

        if (_deadPlayerCount >= _players.Count - 1)
        {
            StartRound();
        }
    }

    public void AddBomb(Player player)
    {
        var cellPosition = _map.GameGrid.WorldToCell(player.transform.position);
        var position = cellPosition + _map.GameTilemap.tileAnchor;
        var bomb = Instantiate(_bombPrefab, position, Quaternion.identity, _map.transform);
        bomb.Initialize(player);
        bomb.OnExplosion.AddListener(OnBombExplode);

        player.UpdateCurrentBombCount(-1);

        _bombs.Add(bomb);
    }

    private void OnBombExplode(Bomb bomb)
    {
        bomb.Player.UpdateCurrentBombCount(1);

        Explosion explosion = Instantiate(_explosionPrefab, bomb.transform.position, Quaternion.identity);
        explosion.Initialize(bomb, _map);
    }

    private void ClearBombs()
    {
        foreach (var bomb in _bombs)
        {
            if (bomb && bomb.gameObject)
                Destroy(bomb.gameObject);
        }

        _bombs.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartRound();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            _map.DestroyAllDestructibleWalls();
        }
    }
}
