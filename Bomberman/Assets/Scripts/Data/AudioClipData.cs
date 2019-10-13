using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipData.asset", menuName = "Bomberman/Audio/AudioClipData")]
public class AudioClipData : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;

    public AudioClip Clip => _clip;
}
