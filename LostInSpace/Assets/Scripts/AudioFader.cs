using UnityEngine;
using System.Collections;

public static class AudioFader
{

    public static IEnumerator FadeClip(AudioSource audioSource, float FadeTime, bool fadeIn)
    {
        float volume;
        if(!fadeIn)
        {
            volume = audioSource.volume;

            while (audioSource.volume > 0)
            {
                audioSource.volume -= volume * Time.deltaTime / FadeTime;

                yield return null;
            }

            audioSource.Stop();
        }
        else if (fadeIn)
        {
            audioSource.volume = 0;
            volume = 1;
            audioSource.Play();

            while (audioSource.volume < 1)
            {
                audioSource.volume += volume * Time.deltaTime / FadeTime;

                yield return null;
            }
        }
    }

}