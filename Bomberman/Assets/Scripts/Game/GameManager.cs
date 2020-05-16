using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Scene reference")]

    [SerializeField] private Camera _camera = null;
    [SerializeField] private UIManager _uiManager = null;
    [SerializeField] private DebugManager _debugManager = null;
    [SerializeField] private AIManager _aiManager = null;

    [Header("Assets reference")]

    [SerializeField] private GameSettings _gameSettings = null;
    [SerializeField] private Player _playerPrefab = null;
    [SerializeField] private AIPlayer _aiPlayerPrefab = null;
    [SerializeField] private MLAIPlayer _mlAIPlayerPrefab = null;
    [SerializeField] private Bomb _bombPrefab = null;
    [SerializeField] private Explosion _explosionPrefab = null;

    public Map Map => _map;

    public List<Player> Players => _players;

    public AIManager AIManager => _aiManager;

    private Map _map = null;
    private List<Player> _players = new List<Player>();
    private List<Bomb> _bombs = new List<Bomb>();
    private List<Explosion> _explosions = new List<Explosion>();
    private int _deadPlayerCount = 0;
    private TimeSpan _time;
    private int _currentMapIndex = 0;
    private Dictionary<int, Vector2Int> _playersPreviousCellPosition = new Dictionary<int, Vector2Int>();

    private void Start()
    {
        LoadMap(_gameSettings.Maps[_currentMapIndex]);
    }

    #region Map

    private void LoadMap(string mapName)
    {
        StartCoroutine(LoadMapSceneCoroutine(_gameSettings.Maps[_currentMapIndex]));
    }

    private void UnloadCurrentMap()
    {
        SceneManager.UnloadSceneAsync(_gameSettings.Maps[_currentMapIndex]);
    }

    private void SwitchMap(string mapName)
    {
        // Make sure to clean all elements from the previous map
        ClearPlayers();
        _uiManager.Clear();
        _aiManager.Clear();

        LoadMap(mapName);
    }

    IEnumerator LoadMapSceneCoroutine(string mapName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mapName, LoadSceneMode.Additive);

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

    #endregion

    private void StartGame(int playerCount = 1)
    {
        List<AIPlayer> aiPlayers = new List<AIPlayer>();

        for (int i = 0; i < playerCount; i++)
        {
            Player player;

            if (i < _gameSettings.MLAIPlayersCount)
            {
                MLAIPlayer mlAIPlayer = Instantiate(_mlAIPlayerPrefab);
                player = mlAIPlayer.Player;
            }
            else if (i < _gameSettings.AIPlayersCount)
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
            player.OnMove.AddListener(OnPlayerMove);
            player.OnDeath.AddListener(OnPlayerDeath);
            player.OnPlantBomb.AddListener(AddBomb);
            _players.Add(player);
        }

        if (aiPlayers.Count > 0)
        {
            _aiManager.Initialize(_map, aiPlayers);
        }

        _uiManager.Initialize(this);

        StartRound();
    }

    #region Round

    private void ClearRoundData()
    {
        _debugManager.Clear();
        _map.Clear();
        ClearBombs();
        ClearExplosions();
    }

    private void StartRound()
    {
        ClearRoundData();

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
            _playersPreviousCellPosition[player.Id] = _map.CellPosition(player.transform.position);
            _map.SetEntityType(EEntityType.Player, player.transform.position);
        }

        StartCoroutine(UpdateTimer());

        _debugManager.Initialize(this);
    }

    #endregion

    #region Player

    public Player GetPlayerAt(Vector2Int cellPosition)
    {
        foreach (Player player in _players)
        {
            if (_map.CellPosition(player.transform.position) == cellPosition)
                return player;
        }

        return null;
    }

    private void OnPlayerMove(Player player)
    {
        Vector2Int cellPosition = _map.CellPosition(player.transform.position);

        if (_playersPreviousCellPosition[player.Id] != cellPosition)
        {
            if (_map.GetEntityType(_playersPreviousCellPosition[player.Id]) != EEntityType.Bomb)
                _map.SetEntityType(EEntityType.None, _map.WorldPosition(_playersPreviousCellPosition[player.Id]));

            _playersPreviousCellPosition[player.Id] = cellPosition;
            
            _map.SetEntityType(EEntityType.Player, player.transform.position);
        }
    }

    private void OnPlayerDeath(Player player)
    {
        _deadPlayerCount++;

        if (_deadPlayerCount >= _players.Count - 1)
        {
            StartRound();
        }
    }

    private void ClearPlayers()
    {
        foreach (var player in _players)
        {
            player.OnMove.RemoveListener(OnPlayerMove);
            Destroy(player.gameObject);
        }

        _players.Clear();
    }

    #endregion

    #region Bomb

    public void AddBomb(Vector2Int cellPosition, float timer, int power)
    {
        var entity = _map.GetEntityType(cellPosition);

        if (entity != EEntityType.None && entity != EEntityType.Player)
            return;

        var position = _map.WorldPosition(cellPosition);
        var bomb = Instantiate(_bombPrefab, position, Quaternion.identity, _map.transform);
        bomb.Initialize(timer, power, _map);
        bomb.OnExplosion.AddListener(OnBombExplode);

        _map.OnBombAdded(bomb);
        _bombs.Add(bomb);
    }

    public void AddBomb(Player player)
    {
        var entity = _map.GetEntityType(player.transform.position);

        if (entity != EEntityType.None && entity != EEntityType.Player)
            return;

        var cellPosition = _map.CellPosition(player.transform.position);
        var position = _map.WorldPosition(cellPosition);
        var bomb = Instantiate(_bombPrefab, position, Quaternion.identity, _map.transform);
        bomb.Initialize(player, _map);
        bomb.OnExplosion.AddListener(OnBombExplode);

        player.UpdateCurrentBombCount(-1);

        // Update map
        _map.OnBombAdded(bomb);

        _bombs.Add(bomb);
    }

    private void OnBombExplode(Bomb bomb)
    {
        bomb.OnExplosion.RemoveListener(OnBombExplode);

        if (bomb.Player != null)
            bomb.Player.UpdateCurrentBombCount(1);

        Explosion explosion = Instantiate(_explosionPrefab, bomb.transform.position, Quaternion.identity);
        explosion.Initialize(bomb, _map);

        _map.OnExplosionAdded(explosion);

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

    #endregion

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

        if (Input.GetKeyDown(KeyCode.B))
        {
            _map.DestroyAllBonus();
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            UnloadCurrentMap();

            _currentMapIndex = (_currentMapIndex + 1) % _gameSettings.Maps.Count;

           SwitchMap(_gameSettings.Maps[_currentMapIndex]);
        }
        else if (Input.GetKeyDown(KeyCode.PageDown))
        {
            UnloadCurrentMap();

            _currentMapIndex--;

            if (_currentMapIndex < 0)
                _currentMapIndex = _gameSettings.Maps.Count - 1;

            SwitchMap(_gameSettings.Maps[_currentMapIndex]);
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int cellPosition = _map.CellPosition(worldPosition);

            if (_map.IsAccessible(cellPosition))
            {
                worldPosition = _map.WorldPosition(cellPosition);
                _map.AddDestructibleWall(worldPosition);
            }
            else
            {
                Debug.LogWarning("Not a valid position for a wall...");
            }
        }
        else if (Input.GetMouseButtonDown(2))
        {
            var position = _camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(_camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                Transform objectHit = hit.transform;

                var destructibleWall = objectHit.gameObject.GetComponent<DestructibleWall>();

                if (destructibleWall != null)
                    destructibleWall.Explode();
                else
                    Debug.LogWarning("No destructible wall found here!");
            }
            else
            {
                AddBomb(_map.CellPosition(position), 2f, 5);
            }
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
