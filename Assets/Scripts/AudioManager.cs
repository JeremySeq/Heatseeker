using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Sound Clips")] [SerializeField]
    public AudioClip pickupSound;
    public AudioClip speedBoostSound;
    public AudioClip shieldSound;
    public AudioClip repulsorSound;
    
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
}