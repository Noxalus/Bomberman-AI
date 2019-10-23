using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to store all configuration of a game
/// Will be edited from the menu just before to start a game
/// </summary>
[CreateAssetMenu(fileName = "GameSettings.asset", menuName = "Bomberman/GameSettings/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Header("Constants")]

    public List<Color> PlayersColor = new List<Color>();
    public float SpeedBonusIncrement = 0.25f;
    public float PlayerSpawnInvincibleTimer = 2.5f; // in seconds
    public float PlayerBaseSpeed = 1f;
    public float PlayerBombBaseTimer = 2f; // in seconds

    [Header("Map configuration")]

    public List<string> Maps = new List<string>{ "Map1", "Map2", "Map3" };
    public int PlayersCount = 4;
    public int AIPlayersCount = 2;
    public float WallDensity = 0.75f;
    public float BonusProbability = 1f;
    public EBonusTypeBoolDictionary AvailableBonus = new EBonusTypeBoolDictionary();

    [Header("Base player stats")]

    public int PlayerBaseBombCount = 1;
    public int PlayerBaseSpeedBonus = 0;
    public int PlayerBaseBombPower = 1;

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
