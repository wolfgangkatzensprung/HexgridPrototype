using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource placeSound;
    [SerializeField] private AudioSource errorSound;
    [SerializeField] private AudioSource selectSound;
    [SerializeField] private AudioSource completedSound;
    [SerializeField] private AudioSource popSound;

    public enum SoundType { Place, Error, Select, Completed, Pop }

    public void PlaySound(SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.Place:
                placeSound?.Play();
                break;
            case SoundType.Error:
                errorSound?.Play();
                break;
            case SoundType.Select:
                selectSound?.Play();
                break;
            case SoundType.Completed:
                completedSound?.Play();
                break;
            case SoundType.Pop:
                popSound?.Play();
                break;
        }
    }
}
