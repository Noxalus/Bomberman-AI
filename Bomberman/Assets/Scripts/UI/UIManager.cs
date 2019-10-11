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

    public void UpdatePlayerPower(int playerId, int power)
    {
        _playersBoardView.UpdatePlayerPower(playerId, power);
    }

    public void UpdatePlayerBombCount(int playerId, int bombCount)
    {
        _playersBoardView.UpdatePlayerBombCount(playerId, bombCount);
    }

    public void UpdatePlayerSpeed(int playerId, int speedBonus)
    {
        _playersBoardView.UpdatePlayerSpeed(playerId, speedBonus);
    }
}
