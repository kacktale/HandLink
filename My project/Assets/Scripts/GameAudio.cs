using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("Editor-assigned sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Editor-authored clips")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip perfectHitClip;
    [SerializeField] private AudioClip pulseWarningClip;
    [SerializeField] private AudioClip pulseBurstClip;
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip healClip;
    [SerializeField] private AudioClip buttonClip;
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip gameOverClip;

    private bool isMuted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
        SetMuted(PlayerPrefs.GetInt("SoundEnabled", 1) == 0);
    }

    private void Start()
    {
        if (bgmSource != null && bgmSource.clip != null && !bgmSource.isPlaying && !isMuted)
        {
            bgmSource.Play();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetMuted(bool muted)
    {
        isMuted = muted;
        if (bgmSource != null)
        {
            bgmSource.mute = muted;
            if (!muted && bgmSource.clip != null && !bgmSource.isPlaying)
            {
                bgmSource.Play();
            }
        }

        if (sfxSource != null)
        {
            sfxSource.mute = muted;
        }
    }

    public void PlayHit(bool perfect)
    {
        PlayOneShot(perfect ? perfectHitClip : hitClip, perfect ? 0.9f : 0.72f);
    }

    public void PlayPulseWarning() => PlayOneShot(pulseWarningClip, 0.7f);
    public void PlayPulseBurst() => PlayOneShot(pulseBurstClip, 0.88f);
    public void PlayCoin() => PlayOneShot(coinClip, 0.9f);
    public void PlayHeal() => PlayOneShot(healClip, 0.78f);
    public void PlayButton() => PlayOneShot(buttonClip, 0.55f);
    public void PlayDamage() => PlayOneShot(damageClip, 0.82f);
    public void PlayGameOver() => PlayOneShot(gameOverClip, 0.92f);

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (!isMuted && sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}
