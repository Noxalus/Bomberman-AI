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

    internal void UpdatePlayerPower(int playerId, int power)
    {
        _playerDataViewInstances[playerId].UpdatePowerCount(power);
    }

    public void UpdatePlayerBombCount(int playerId, int bombCount)
    {
        _playerDataViewInstances[playerId].UpdateBombCount(bombCount);
    }

    internal void UpdatePlayerSpeed(int playerId, int speed)
    {
        _playerDataViewInstances[playerId].UpdateSpeedCount(speed);
    }
}
