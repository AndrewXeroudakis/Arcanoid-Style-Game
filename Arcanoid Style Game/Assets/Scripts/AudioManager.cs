using System;
using UnityEngine.Audio;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public AudioMixer audioMixer;

    public Sound[] sounds;

    public static AudioManager audioManager;
    
	void Awake () {

        if (audioManager == null)
        {
            DontDestroyOnLoad(gameObject);
            audioManager = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
	}

    void Start()
    {
        PlayMusic("MainTheme");
        //PlaySoundAtPoint(RandomizeSfx("HitPaddle"), transform.position);
    }

    AudioSource PlayMusic(string musicName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == musicName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + musicName + " not found!");
            return null;
        }
        else
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            UpdateAudioSource(audioSource, s);

            audioSource.Play();

            return audioSource;
        }
    }

    // Used for non-persistent sounds (sfx, ambience etc.)
    public GameObject PlaySoundAtPoint(string soundName, Vector3 pos)
    {
        float volume = GetParameter("sfxVolume");
        if (volume <= -80) { return null; }
        else
        {
            Sound s = Array.Find(sounds, sound => sound.name == soundName);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + soundName + " not found!");
                return null;
            }
            else
            {
                GameObject sound = new GameObject("Sound");
                sound.transform.position = pos;

                AudioSource audioSource = sound.AddComponent<AudioSource>();
                UpdateAudioSource(audioSource, s);

                if (audioSource.loop == false) { Destroy(sound, audioSource.clip.length); }

                audioSource.Play();

                return sound;
            }
        }
    }

    public string RandomizeSfx(params string[] sfx)
    {
        int randomIndex = UnityEngine.Random.Range(0, sfx.Length);
        return sfx[randomIndex];
    }

    public float RandomizePitch(float pitch, float pitchRange)
    {
        return UnityEngine.Random.Range(pitch - pitchRange, pitch + pitchRange);
    }

    /*public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20);
    }*/

    public void SetSfxVolume(float volume)
    {
        audioMixer.SetFloat("sfxVolume", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(volume) * 20);
    }

    public float GetParameter(string parameter)
    {
        float value;
        bool result = audioMixer.GetFloat(parameter, out value);
        if (result == true) { return value; }
        else { return 0f; }
    }

    void UpdateAudioSource(AudioSource audioSource, Sound s)
    {
        audioSource.clip = s.clip;
        audioSource.outputAudioMixerGroup = s.outputAudioMixerGroup;
        audioSource.priority = s.priority;
        audioSource.volume = s.volume;
        if (s.pitchRange > 0) { audioSource.pitch = RandomizePitch(s.pitch, s.pitchRange); }
        else { audioSource.pitch = s.pitch; }
        audioSource.spatialBlend = s.spatialBlend;
        audioSource.loop = s.loop;
    }
}

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public AudioMixerGroup outputAudioMixerGroup;
    [Range(0, 256)]
    public int priority;
    [Range(0f, 1f)]
    public float volume;
    [Range(-3f, 3f)]
    public float pitch;
    [Range(0f, 1f)]
    public float pitchRange = 0.05f;
    [Range(0f, 1f)]
    public float spatialBlend;
    public bool loop;
}
