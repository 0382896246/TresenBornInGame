using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip correctClip;
    public AudioClip wrongClip;
    public AudioClip bgmClip;

    [Header("Volumes")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.6f;

    [Header("Options")]
    [SerializeField] bool autoPlayBGM = true;   // tự phát khi scene load

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    void Awake()
    {
        // Xoá phần dontDestroy, giờ mỗi scene có AudioManager riêng
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.spatialBlend = 0f; // 2D
    }

    void Start()
    {
        if (autoPlayBGM) PlayBGM();
    }

    public void PlayCorrect()
    {
        if (correctClip) sfxSource.PlayOneShot(correctClip, sfxVolume);
    }

    public void PlayWrong()
    {
        if (wrongClip) sfxSource.PlayOneShot(wrongClip, sfxVolume);
    }

    public void PlayBGM()
    {
        if (!bgmClip) return;
        if (bgmSource.clip != bgmClip) bgmSource.clip = bgmClip;
        bgmSource.volume = bgmVolume;
        if (!bgmSource.isPlaying) bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource.isPlaying) bgmSource.Stop();
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
    }

    public void SetBGMVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        if (bgmSource) bgmSource.volume = bgmVolume;
    }
}
