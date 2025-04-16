using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource bgmSource;
    public AudioSource voiceSource;

    // 🎵 Grassland用のBGMをここにセット
    public AudioClip grasslandClip;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        PlayBGM(grasslandClip); // シーン開始時に再生！
    }

    public void PlayBGM(AudioClip clip, float volume = 0.8f)
    {
        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayVoice(AudioClip clip, float pitch = 1.0f)
    {
        voiceSource.pitch = pitch;
        voiceSource.PlayOneShot(clip);
    }
}
