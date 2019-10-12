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
    public float WallDensity = 0.75f;
    public EBonusTypeBoolDictionary AvailableBonus = new EBonusTypeBoolDictionary();
    public float BonusProbability = 1f;
    public float SpeedBonusIncrement = 0.25f;

    public List<EBonusType> GetAvailableBonus()
    {
        List<EBonusType> availableBonusType = new List<EBonusType>();

        foreach (var bonusType in AvailableBonus.Keys)
        {
            if (AvailableBonus[bonusType])
                availableBonusType.Add(bonusType);
        }

        return availableBonusType;
    }
}
