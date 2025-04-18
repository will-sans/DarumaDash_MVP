using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource bgmSource;
    public AudioSource voiceSource;
    public AudioClip grasslandClip;
    public AudioClip heartbeatClip;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        PlayBGM(grasslandClip);
    }

    public void PlayBGM(AudioClip clip, float volume = 0.3f)
    {
        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayVoice(AudioClip clip, float pitch = 1.0f)
    {
        voiceSource.pitch = pitch;
        voiceSource.volume = 1.0f;
        voiceSource.PlayOneShot(clip);
    }

    public void PlayDarumaBGM()
    {
        PlayBGM(heartbeatClip, 0.15f);
    }
}