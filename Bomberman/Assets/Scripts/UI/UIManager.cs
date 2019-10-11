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
}
