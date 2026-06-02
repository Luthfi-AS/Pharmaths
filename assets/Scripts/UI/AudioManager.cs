using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips - BGM")]
    public AudioClip bgm1;
    public AudioClip bgm2;

    [Header("Audio Clips - SFX")]
    public AudioClip clickGeneral;
    public AudioClip selectRole;
    public AudioClip typingSingle;
    public AudioClip typingMulti;
    public AudioClip transitionAR;
    public AudioClip succeedStatus;
    public AudioClip failedStatus;

    private void Awake()
    {
        // Sistem Singleton agar Audio tidak mati saat pindah scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- FUNGSI UNTUK MEMUTAR BGM (LOOPING) ---
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return; // Gak perlu restart kalau lagunya sama

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource.Stop();

    // --- FUNGSI UNTUK MEMUTAR SFX (TUMPAH TINDIH AMAN) ---
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            // PlayOneShot bikin sfx bisa bunyi barengan tanpa saling memotong
            sfxSource.PlayOneShot(clip); 
        }
    }
}