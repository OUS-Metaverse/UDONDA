using UdonSharp;
using UnityEngine;

enum Status
{
    Playing,
    FadingOut,
    FadingIn,
}

public class BGMManager : UdonSharpBehaviour
{
    [SerializeField] AudioSource bgm;
    [SerializeField] float maxVolume = 1.0f;
    [SerializeField] float minVolume = 0.0f;
    [SerializeField] float fadeOutTime = 1.0f;
    [SerializeField] float fadeInTime = 3.0f;


    Status _status = Status.Playing;

    public void FadeOutBGM()
    {
        _status = Status.FadingOut;
    }

    public void FadeInBGM()
    {
        _status = Status.FadingIn;
    }

    void Update()
    {
        if (_status == Status.FadingOut)
        {
            bgm.volume -= Time.deltaTime / fadeOutTime;
            if (bgm.volume <= minVolume)
            {
                bgm.volume = minVolume;
                _status = Status.Playing;
            }
        }
        else if (_status == Status.FadingIn)
        {
            bgm.volume += Time.deltaTime / fadeInTime;
            if (bgm.volume >= maxVolume)
            {
                bgm.volume = maxVolume;
                _status = Status.Playing;
            }
        }
    }
}
