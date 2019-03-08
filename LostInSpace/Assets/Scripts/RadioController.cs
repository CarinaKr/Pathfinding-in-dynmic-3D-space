using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class RadioController : MonoBehaviour {

    public Text radioChannelText;
    public AudioClip[] channels;

    private int channel;
    private AudioSource aSource;
    private float fadeTime = 1.5f;

	// Use this for initialization
	void Awake () {
        channel = channels.Length+1;
        aSource = GetComponent<AudioSource>();
        SwapChannel(channel);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("PreviousChannel"))
        {
            //prev. ch
            channel--;
            if(channel < 0 )
            {
                channel = channels.Length;
            }
            StartCoroutine(SwapChannel(channel));
        }
        else if (Input.GetButtonDown("NextChannel"))
        {
            //next ch.
            channel++;
            channel %= channels.Length+1;
            StartCoroutine(SwapChannel(channel));
        }
	}

    IEnumerator SwapChannel(int channel)
    {
        if(channel < channels.Length)
        {
            StartCoroutine(AudioFader.FadeClip(aSource, fadeTime, false));
            yield return new WaitForSeconds(fadeTime);
            aSource.clip = channels[channel];
            radioChannelText.text = "Radio: 0" + (channel+1); 
            StartCoroutine(AudioFader.FadeClip(aSource, fadeTime, true));
        }
        else
        {
            StartCoroutine(AudioFader.FadeClip(aSource, fadeTime, false));
            radioChannelText.text = "Radio: Off";
        }
    }
}
