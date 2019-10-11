using System.Collections.Generic;
using UnityEngine;

public class Bonus : MonoBehaviour
{
    [SerializeField] private GameSettings _gameSettings = null;
    [SerializeField] private EBonusTypeBonusSpriteDictionary _bonusSpritesDictionary = new EBonusTypeBonusSpriteDictionary();
    [SerializeField] private SpriteRenderer _normalSpriteRenderer = null;
    [SerializeField] private SpriteRenderer _highlightedSpriteRenderer = null;

    private EBonusType _type = EBonusType.None;

    private void Awake()
    {
        List<EBonusType> availableBonusType = new List<EBonusType>();

        foreach (var bonusType in _gameSettings.AvailableBonus.Keys)
        {
            if (_gameSettings.AvailableBonus[bonusType])
                availableBonusType.Add(bonusType);
        }

        Initalize(availableBonusType);
    }

    public void Initalize(List<EBonusType> availableBonusType)
    {
        _type = availableBonusType[Random.Range(0, availableBonusType.Count)];

        _normalSpriteRenderer.sprite = _bonusSpritesDictionary[_type].Normal;
        _highlightedSpriteRenderer.sprite = _bonusSpritesDictionary[_type].Highlighted;
    }
}
