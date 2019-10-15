using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] PlayersBoardView _playersBoardView = null;
    [SerializeField] TimerView _timer = null;

    public void Initialize(List<Player> players)
    {
        _playersBoardView.Initialize(players);
    }

    public void UpdateTimer(TimeSpan time)
    {
        _timer.UpdateTimer(time);
    }

    public void Clear()
    {
        _playersBoardView.Clear();
    }
}
