using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Scene reference")]

    [SerializeField] private UIManager _uiManager = null;

    [Header("Assets reference")]

    [SerializeField] private Player _playerPrefab = null;
    [SerializeField] private GameSettings _gameSettings = null;

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
            player.Initialize(i, _gameSettings.PlayersColor[i], this);
            player.OnDeath.AddListener(OnPlayerDeath);
            _players.Add(player);
        }

        _uiManager.Initialize(_players);

        StartRound();
    }

    private void StartRound()
    {
        MusicManager.Instance.PlayMusic(_map.Music);
        _map.GenerateDestrucibleWalls(_gameSettings.WallDensity);
        _deadPlayerCount = 0;

        foreach (var player in _players)
        {
            player.Respawn(_map.GetSpawnPosition(player.Id));
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

    public void AddBomb(Bomb bomb, Vector3 worldPosition)
    {
        var cellPosition = _map.GameGrid.WorldToCell(worldPosition);
        bomb.transform.position = cellPosition + _map.GameTilemap.tileAnchor;

        _bombs.Add(bomb);
    }
}
