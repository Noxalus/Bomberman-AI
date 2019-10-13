using UnityEngine;

public class MusicManager : Service<MusicManager>
{
    [SerializeField] private AudioSource _audioSource = null;

    public void PlayMusic(AudioClip audioClip, bool loop = true)
    {
        _audioSource.Stop();

        _audioSource.clip = audioClip;
        _audioSource.loop = loop;

        _audioSource.Play();
    }
}
