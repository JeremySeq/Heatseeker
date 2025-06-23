using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public static MusicManager Instance;
    public bool enableMusic = true;
    private AudioClip[] musicClips;
    private int currentTrackIndex = -1;

    void Start()
    {

        Instance = this;
        // Load all AudioClips from the Resources/Music folder
        musicClips = Resources.LoadAll<AudioClip>("Music");

        if (musicClips.Length == 0)
        {
            Debug.LogWarning("No music clips found in Resources/Music!");
            return;
        }

        PlayRandomTrack();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayRandomTrack();
        }
    }

    void PlayRandomTrack()
    {
        if (!this.enableMusic) return;
        int newIndex = Random.Range(0, musicClips.Length);
        while (newIndex == currentTrackIndex && musicClips.Length > 1)
        {
            newIndex = Random.Range(0, musicClips.Length);
        }

        currentTrackIndex = newIndex;
        audioSource.clip = musicClips[currentTrackIndex];
        audioSource.Play();
    }
}