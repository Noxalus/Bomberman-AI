using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] PlayersBoardView _playersBoardView = null;
    [SerializeField] TimerView _timer = null;
    [SerializeField] Minimap _minimap = null;

    private GameManager _gameManager;

    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;

        _playersBoardView.Initialize(gameManager.Players);
        _minimap.Initialize(gameManager.Map);
    }

    public void UpdateTimer(TimeSpan time)
    {
        _timer.UpdateTimer(time);
    }

    public void Clear()
    {
        _playersBoardView.Clear();
        _minimap.Clear();
    }
}
