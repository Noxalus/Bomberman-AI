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
    [SerializeField] private AIManager _aiManager = null;

    [Header("Assets reference")]

    [SerializeField] private GameSettings _gameSettings = null;
    [SerializeField] private Player _playerPrefab = null;
    [SerializeField] private AIPlayer _aiPlayerPrefab = null;
    [SerializeField] private Bomb _bombPrefab = null;
    [SerializeField] private Explosion _explosionPrefab = null;

    public Map Map => _map;

    public List<Player> Players => _players;

    private Map _map = null;
    private List<Player> _players = new List<Player>();
    private List<Bomb> _bombs = new List<Bomb>();
    private List<Explosion> _explosions = new List<Explosion>();
    private int _deadPlayerCount = 0;
    private TimeSpan _time;

    private void Start()
    {
        StartCoroutine(LoadMapScene());
    }

    IEnumerator LoadMapScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_gameSettings.SelectedMapName, LoadSceneMode.Additive);

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
        List<AIPlayer> aiPlayers = new List<AIPlayer>();

        for (int i = 0; i < playerCount; i++)
        {
            Player player;

            if (i < _gameSettings.AIPlayersCount)
            {
                AIPlayer aiPlayer = Instantiate(_aiPlayerPrefab);
                aiPlayers.Add(aiPlayer);
                player = aiPlayer;
            }
            else
            {
                player = Instantiate(_playerPrefab);
            }

            player.Initialize(i, _gameSettings.PlayersColor[i]);
            player.OnDeath.AddListener(OnPlayerDeath);
            player.OnPlantBomb.AddListener(AddBomb);
            _players.Add(player);
        }

        if (aiPlayers.Count > 0)
        {
            _aiManager.Initialize(_map, aiPlayers);
        }

        _uiManager.Initialize(_players);

        StartRound();
    }

    private void StartRound()
    {
        _map.Clear();
        ClearBombs();
        ClearExplosions();

        StopAllCoroutines();
        StopCoroutine(UpdateTimer());

        _time = TimeSpan.FromSeconds(0);
        _uiManager.UpdateTimer(_time);

        MusicManager.Instance.PlayMusic(_map.Music);
        _map.GenerateDestrucibleWalls(_gameSettings.WallDensity);
        _deadPlayerCount = 0;

        foreach (var player in _players)
        {
            player.Spawn(_map.GetSpawnPosition(player.Id));
        }

        StartCoroutine(UpdateTimer());
    }

    private void OnPlayerDeath(Player player)
    {
        _deadPlayerCount++;

        if (_deadPlayerCount >= _players.Count - 1)
        {
            StartRound();
        }
    }

    public void AddBomb(Player player)
    {
        var entity = _map.GetEntityType(player.transform.position);

        if (entity != EEntityType.None)
            return;

        var cellPosition = _map.GameGrid.WorldToCell(player.transform.position);
        var position = cellPosition;
        var bomb = Instantiate(_bombPrefab, position, Quaternion.identity, _map.transform);
        bomb.Initialize(player);
        bomb.OnExplosion.AddListener(OnBombExplode);

        player.UpdateCurrentBombCount(-1);

        _map.SetEntityType(EEntityType.Bomb, bomb.transform.position);

        _bombs.Add(bomb);
    }

    private void OnBombExplode(Bomb bomb)
    {
        bomb.OnExplosion.RemoveListener(OnBombExplode);

        bomb.Player.UpdateCurrentBombCount(1);

        Explosion explosion = Instantiate(_explosionPrefab, bomb.transform.position, Quaternion.identity);
        explosion.Initialize(bomb, _map);

        _map.SetEntityType(EEntityType.Explosion, explosion.transform.position);

        _explosions.Add(explosion);
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

    private void ClearExplosions()
    {
        foreach (var explosion in _explosions)
        {
            if (explosion && explosion.gameObject)
                Destroy(explosion.gameObject);
        }

        _explosions.Clear();
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

    private IEnumerator UpdateTimer()
    {
        float refreshTime = 1f;

        while (true)
        {
            yield return new WaitForSeconds(refreshTime);
            _time += TimeSpan.FromSeconds(refreshTime);
            _uiManager.UpdateTimer(_time);
        }
    }
}
