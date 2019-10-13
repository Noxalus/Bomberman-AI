using UnityEngine;

public class SoundManager : Service<SoundManager>
{
    [Header("Inner references")]

    [SerializeField] private AudioSource _audioSource = null;

    [Header("Assets")]

    [SerializeField] private AudioClip _playerDeathSound = null;
    [SerializeField] private AudioClip _bombExplodeSound = null;
    [SerializeField] private AudioClip _bonusPickedupSound = null;

    public void PlaySound(string soundName)
    {
        switch (soundName)
        {
            case "PlayerDeath":
                _audioSource.PlayOneShot(_playerDeathSound);
                break;
            case "BombExplode":
                _audioSource.PlayOneShot(_bombExplodeSound);
                break;
            case "BonusPickup":
                _audioSource.PlayOneShot(_bonusPickedupSound);
                break;
        }
    }
}
