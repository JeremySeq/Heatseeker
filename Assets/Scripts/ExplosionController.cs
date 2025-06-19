using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    public AudioClip explosionSound;

    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        if (explosionSound != null)
        {
            _audioSource.clip = explosionSound;
            _audioSource.spatialBlend = 1f;
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
            _audioSource.maxDistance = 50f;
            _audioSource.Play();
        }

        Animator animator = GetComponent<Animator>();
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        Destroy(gameObject, animationLength);
    }
}