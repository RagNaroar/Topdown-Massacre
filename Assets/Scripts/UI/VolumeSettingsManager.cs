using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;
public class VolumeSettingsManager : MonoBehaviour
{
[Serializable]
public class AudioChannel
{
public string name;
public string mixerParameter;
public string prefsKey;
public Slider slider;
public float DefaultValue = 0.3f;
}
[Header("Audio Mixer Reference")]
[SerializeField] public AudioMixer audioMixer;

[Header("Channels Setup")]
[SerializeField] private AudioChannel[] channels;

    private void Start()
    {
         float value = PlayerPrefs.GetFloat("MasterVolume", 0.3f);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        InitializeChannels();
    }

    private void InitializeChannels()
    {
        foreach (var channel in channels)
        {
            if(channel.slider == null) continue;

            float savedVolume = PlayerPrefs.GetFloat(channel.prefsKey, channel.DefaultValue);
            channel.slider.value = savedVolume;
            ApplyVolume(channel.mixerParameter, savedVolume);
            AudioChannel currentChannel = channel; // Capture the current channel in a local variable
            channel.slider.onValueChanged.AddListener((value) => OnSliderValueChanged(currentChannel, value));
        }
    }

    private void OnSliderValueChanged(AudioChannel channel, float value)
    {
        ApplyVolume(channel.mixerParameter, value);
        PlayerPrefs.SetFloat(channel.prefsKey, value);
    }

    private void ApplyVolume(string mixerParameter, float value)
    {
        float clampedValue = Mathf.Max(value, 0.0001f);
        audioMixer.SetFloat(mixerParameter, Mathf.Log10(clampedValue) * 20);
    }

     public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);

        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}
