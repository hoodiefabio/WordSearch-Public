using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicSlider : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider musicSlider;

    const string Mixer_Music = "MusicVolume";

    private void Start()
    {
        float value;
        bool result = mixer.GetFloat(Mixer_Music, out value);
        musicSlider.value = Mathf.Pow(10, (value / 20));
    }

    void Awake()
    {
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    void SetMusicVolume(float volume)
    {
        mixer.SetFloat(Mixer_Music, Mathf.Log10(volume)*20);    
    }

   
}
