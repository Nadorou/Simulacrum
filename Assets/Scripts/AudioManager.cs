using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    public static AudioManager instance;    

    [System.Serializable]
    public struct KeyedAudioHolder
    {
        public string key;
        public AudioClip audioClip;
    }

    [SerializeField]
    private AudioMixer m_audioMixer;

    [SerializeField]
    private AudioSource m_musicSource;

    [SerializeField]
    [Tooltip("Every key needs to be unique")]
    private KeyedAudioHolder[] audioClips;
    //This is the dict that we access to grab a given audio clip by its key. It is populated on Start().
    public Dictionary<string, AudioClip> audioDict = new Dictionary<string, AudioClip>();

    private Coroutine m_musicFaderCR;
    private float m_musicFadeVolume = 0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        foreach (KeyedAudioHolder entry in audioClips)
        {
            audioDict.Add(entry.key, entry.audioClip);
        }
    }

    public static float ConvertLinearVolumeToDecibels(float volume)
    {
        float volumeInDecibels;
        if (volume < 0.0001f)
        {
            volumeInDecibels = -80f;
        }
        else
        {
            volumeInDecibels = 20 * Mathf.Log10(volume);
        }
        return volumeInDecibels;
    }

    public static float ConvertDecibelsToLinearVolume(float decibels)
    {
        float linearVolume = Mathf.Pow(10f, decibels / 20f);
        return linearVolume;
    }

    public void RequestAudio(string key, AudioSource source)
    {
        if (DoesRequestedClipExist(key))
        {
            AudioClip requestedAudioClip = audioDict[key];
            PlayAudioClip(requestedAudioClip, source);
        } else
        {
            Debug.Log("Requested clip '" + key + "' but it does not exist within the audio manager");
        }
    }

    public void PlayAudioClip(AudioClip clip, AudioSource source)
    {
        source.PlayOneShot(clip);
    }

    public void FadeOutMusic(float time)
    {
        FadeRoutineCheck();

        StartCoroutine(FadeMusic(m_musicFadeVolume, 0f, time));
    }

    public void FadeInMusic(float time)
    {
        FadeRoutineCheck();

        StartCoroutine(FadeMusic(m_musicFadeVolume, 1f, time));
    }

    public void SwapMusicAndFade(AudioClip musicClip, float time)
    {
        m_musicSource.Stop();

        if(musicClip != null)
        {
            m_musicSource.clip = musicClip;
            m_musicSource.Play();            
        }

        FadeInMusic(time);
    }

    private IEnumerator FadeMusic(float fadeFrom, float fadeTo, float fadeTime)
    {
        float timeSpent = 0;

        while(timeSpent < fadeTime)
        {
            timeSpent += Time.deltaTime;
            m_musicFadeVolume = Mathf.Lerp(fadeFrom, fadeTo, timeSpent / fadeTime);
            m_audioMixer.SetFloat("MusicFader", ConvertLinearVolumeToDecibels(m_musicFadeVolume));
            yield return null;
        }
    }

    private void FadeRoutineCheck()
    {
        if (m_musicFaderCR != null)
        {
            StopCoroutine(m_musicFaderCR);
        }
    }

    private bool DoesRequestedClipExist(string audioKey)
    {
        if (audioDict.ContainsKey(audioKey))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
