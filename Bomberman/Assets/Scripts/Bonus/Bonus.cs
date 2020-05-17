using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[System.Serializable]
public class BonusEvent : UnityEvent<Bonus> { }

public class Bonus : MonoBehaviour
{
    [Header("Events")]

    public BonusEvent OnExplode;
    public BonusEvent OnDestroy;

    [Header("Inner references")]

    [SerializeField] private EBonusTypeBonusSpriteDictionary _bonusSpritesDictionary = new EBonusTypeBonusSpriteDictionary();
    [SerializeField] private SpriteRenderer _normalSpriteRenderer = null;
    [SerializeField] private SpriteRenderer _highlightedSpriteRenderer = null;
    [SerializeField] private Animator _animator = null;

    private EBonusType _type = EBonusType.None;
    private bool _isInvincible = true;

    private void Destroy()
    {
        OnDestroy?.Invoke(this);
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

            if (player)
            {
                SoundManager.Instance.PlaySound("BonusPickup");
                ApplyEffect(player);
                Destroy();
            }
            else
            {
                throw new Exception("Missing Player component on this entity.");
            }
        }
        else if (collision.tag == "Explosion")
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (!_isInvincible)
        {
            _animator.SetBool("InFire", true);
            OnExplode?.Invoke(this);
        }
    }

    private void ApplyEffect(Player player)
    {
        player.PickUpBonus(_type);
    }
}
