using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayersBoardView : MonoBehaviour
{
    [SerializeField] PlayerDataView _playerDataViewPrefab = null;

    private Dictionary<int, PlayerDataView> _playerDataViewInstances = new Dictionary<int, PlayerDataView>();

    public void Initialize(List<Player> players)
    {
        foreach (var player in players)
        {
            var playerDataView = Instantiate(_playerDataViewPrefab, transform);
            playerDataView.Initialize(player);

            _playerDataViewInstances.Add(player.Id, playerDataView);
        }
    }

    public void UpdatePlayerBombCount(int playerId, int bombCount)
    {
        _playerDataViewInstances[playerId].UpdateBomb(bombCount);
    }
}
