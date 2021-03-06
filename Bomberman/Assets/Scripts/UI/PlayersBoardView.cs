﻿using System.Collections.Generic;
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

    public void Clear()
    {
        foreach (var _playerDataView in _playerDataViewInstances.Values)
            Destroy(_playerDataView.gameObject);

        _playerDataViewInstances.Clear();
    }
}
