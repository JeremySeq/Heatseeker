using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Sound Clips - In-Game")]
    [SerializeField] public AudioClip pickupSound;
    [SerializeField] public AudioClip speedBoostSound;
    [SerializeField] public AudioClip shieldSound;
    [SerializeField] public AudioClip repulsorSound;

    [Space(10)]
    [Header("Sound Clips - UI")]
    [SerializeField] public AudioClip clickSound;
    [SerializeField] public AudioClip hoverSound;

    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void PlaySound(AudioClip clip, float volume = 1f, int priority = 128)
    {
        GameObject soundObject = new GameObject("TempAudio");
        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = this.sfxGroup;
        source.clip = clip;
        source.volume = volume;
        source.priority = priority;
        source.Play();
        Destroy(soundObject, clip.length);
    }
    
    public void PlayUISound(AudioClip clip, float volume = 1f)
    {
        GameObject soundObject = new GameObject("TempUIAudio");
        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = Instance.sfxGroup;
        source.clip = clip;
        source.volume = volume;
        source.priority = 128;
        source.spatialBlend = 0f;
        source.Play();
        Destroy(soundObject, clip.length);
    }
}