using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bonus : MonoBehaviour
{
    [SerializeField] private EBonusTypeBonusSpriteDictionary _bonusSpritesDictionary = new EBonusTypeBonusSpriteDictionary();
    [SerializeField] private SpriteRenderer _normalSpriteRenderer = null;
    [SerializeField] private SpriteRenderer _highlightedSpriteRenderer = null;

    private EBonusType _type = EBonusType.None;

    private void Destroy()
    {
        Destroy(gameObject);
    }

    public void Initalize(List<EBonusType> availableBonusType)
    {
        _type = availableBonusType[Random.Range(0, availableBonusType.Count)];

        _normalSpriteRenderer.sprite = _bonusSpritesDictionary[_type].NormalSprite;
        _highlightedSpriteRenderer.sprite = _bonusSpritesDictionary[_type].HighlightedSprite;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Player player = collision.gameObject.GetComponent<Player>();
            ApplyEffect(player);
            Destroy();
        }
    }

    private void ApplyEffect(Player player)
    {
        switch (_type)
        {
            case EBonusType.None:
                Debug.LogError("This bonus has no type.");
                break;
            case EBonusType.Power:
                player.UpdatePower(1);
                break;
            case EBonusType.Bomb:
                player.UpdateBombCount(1);
                break;
            case EBonusType.Speed:
                player.UpdateSpeedBonus(1);
                break;
            case EBonusType.Bad:
                // TODO
                break;
            case EBonusType.Score:
                player.UpdateScore(1);
                break;
            default:
                Debug.LogError("Unknow bonus type.");
                break;
        }
    }
}
