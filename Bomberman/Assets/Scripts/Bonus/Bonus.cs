using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bonus : MonoBehaviour
{
    [SerializeField] private EBonusTypeBonusSpriteDictionary _bonusSpritesDictionary = new EBonusTypeBonusSpriteDictionary();
    [SerializeField] private SpriteRenderer _normalSpriteRenderer = null;
    [SerializeField] private SpriteRenderer _highlightedSpriteRenderer = null;
    [SerializeField] private Animator _animator = null;

    private EBonusType _type = EBonusType.None;
    private bool _isInvincible = true;

    private void Destroy()
    {
        Destroy(gameObject);
    }

    public void Initalize(List<EBonusType> availableBonusType)
    {
        _type = availableBonusType[Random.Range(0, availableBonusType.Count)];

        _normalSpriteRenderer.sprite = _bonusSpritesDictionary[_type].NormalSprite;
        _highlightedSpriteRenderer.sprite = _bonusSpritesDictionary[_type].HighlightedSprite;

        _isInvincible = true;
    }

    public void OnSpawnAnimationFinish()
    {
        _isInvincible = false;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Player player = collision.gameObject.GetComponent<Player>();
            SoundManager.Instance.PlaySound("BonusPickup");
            ApplyEffect(player);
            Destroy();
        }
        else if (collision.tag == "Explosion")
        {
            if (!_isInvincible)
            {
                _animator.SetBool("InFire", true);
            }
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
                player.UpdateMaxBombCount(1);
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
