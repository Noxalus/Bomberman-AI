using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static AudioSource _audioSource = null;
    private static AudioClip _playerDeathSound = null;
    private static AudioClip _bombExplodeSound = null;
    private static AudioClip _bonusPickedupSound = null;

    private void Start()
    {
        _playerDeathSound = Resources.Load<AudioClip>("death");
        _bombExplodeSound = Resources.Load<AudioClip>("boom");
        _bonusPickedupSound = Resources.Load<AudioClip>("item");

        _audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(string soundName)
    {
        switch (soundName)
        {
            case "playerDeath":
                _audioSource.PlayOneShot(_playerDeathSound);
                break;
            case "bombExplode":
                _audioSource.PlayOneShot(_bombExplodeSound);
                break;
            case "bonusPickup":
                _audioSource.PlayOneShot(_bonusPickedupSound);
                break;
        }
    }
}
