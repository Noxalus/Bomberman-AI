using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Scene reference")]

    [SerializeField] private WallGenerator _wallGenerator = null;

    [Header("Assets reference")]

    [SerializeField] private Player _playerPrefab = null;

    private Map _map = null;
    private List<Player> _players = new List<Player>();

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
            _wallGenerator.GenerateWalls(1f);

            StartGame(4);
        }
    }

    private void StartGame(int playerCount = 1)
    {
        for (int i = 0; i < playerCount; i++)
        {
            var player = Instantiate(_playerPrefab, _map.GetSpawnPosition(i), Quaternion.identity);
            _players.Add(player);
        }
    }
}
