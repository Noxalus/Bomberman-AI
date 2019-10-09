using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] PlayersBoardView _playersBoardView = null;

    public void Initialize(List<Player> players)
    {
        _playersBoardView.Initialize(players);
    }

    public void UpdatePlayerBombCount(int playerId, int bombCount)
    {
        _playersBoardView.UpdatePlayerBombCount(playerId, bombCount);
    }
}
