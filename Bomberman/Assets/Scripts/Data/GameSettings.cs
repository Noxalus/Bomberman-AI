using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to store all configuration of a game
/// Will be edited from the menu just before to start a game
/// </summary>
[CreateAssetMenu(fileName = "GameSettings.asset", menuName = "Bomberman/GameSettings/GameSettings")]
public class GameSettings : ScriptableObject
{
    public List<Color> PlayersColor = new List<Color>();
}
