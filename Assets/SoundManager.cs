
using UdonSharp;
using UnityEngine;

public class SoundManager : UdonSharpBehaviour
{
    AudioSource _audioSource;

    [SerializeField] AudioClip keyStrokeSound;
    [SerializeField] AudioClip missSound;
    [SerializeField] AudioClip correctSound;
    [SerializeField] AudioClip extendTimeSound;
    [SerializeField] AudioClip gameStartSound;
    [SerializeField] AudioClip gameEndSound;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayKeyStrokeSound()
    {
        _audioSource.PlayOneShot(keyStrokeSound);
    }

    public void PlayMissSound()
    {
        _audioSource.PlayOneShot(missSound);
    }

    public void PlayCorrectSound()
    {
        _audioSource.PlayOneShot(correctSound);
    }

    public void PlayExtendTimeSound()
    {
        _audioSource.PlayOneShot(extendTimeSound);
    }

    public void PlayGameStartSound()
    {
        _audioSource.PlayOneShot(gameStartSound);
    }

    public void PlayGameEndSound()
    {
        _audioSource.PlayOneShot(gameEndSound);
    }
}
