using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PostProcessingFX : MonoBehaviour
{
    public static PostProcessingFX Instance;
    
    public Volume volume;
    private ChromaticAberration _chromaticAberration;
    private Coroutine _currentEffect;

    void Awake()
    {
        Instance = this;
        if (volume.profile.TryGet(out _chromaticAberration))
        {
            _chromaticAberration.intensity.value = 0f;
            _chromaticAberration.active = false;
        }
    }
    
    void Start()
    {
        // get the ChromaticAberration override from the volume
        if (volume.profile.TryGet(out _chromaticAberration))
        {
            _chromaticAberration.intensity.value = 0f;
            _chromaticAberration.active = false;
        }
        else
        {
            Debug.LogWarning("Chromatic Aberration not found in volume profile.");
        }
    }
    
    public static void PulseChromaticAberration(float fadeInTime = 0.3f, float holdTime = 0.2f, float fadeOutTime = 0.3f, float maxIntensity = 0.8f)
    {
        if (Instance != null && Instance._chromaticAberration != null)
        {
            Instance.StartCoroutine(Instance.DoPulseChromaticAberration(fadeInTime, holdTime, fadeOutTime, maxIntensity));
        }
    }
    
    private IEnumerator DoPulseChromaticAberration(float fadeIn, float hold, float fadeOut, float intensity)
    {
        _chromaticAberration.active = true;
        yield return StartCoroutine(FadeChromaticAberration(0f, intensity, fadeIn));
        yield return new WaitForSeconds(hold);
        yield return StartCoroutine(FadeChromaticAberration(intensity, 0f, fadeOut));
        _chromaticAberration.active = false;
    }

    public static void FadeInChromaticAberration(float duration = 1f, float targetIntensity = 1f, bool disableAfter = false)
    {
        if (Instance._currentEffect != null)
            Instance.StopCoroutine(Instance._currentEffect);

        Instance._chromaticAberration.active = true;
        Instance._currentEffect = Instance.StartCoroutine(Instance.FadeChromaticAberration(0f, targetIntensity, duration, disableAfter));
    }

    public void FadeOutChromaticAberration(float duration = 1f)
    {
        if (_currentEffect != null)
            StopCoroutine(_currentEffect);

        _currentEffect = StartCoroutine(FadeChromaticAberration(_chromaticAberration.intensity.value, 0f, duration, disableAfter: true));
    }

    private IEnumerator FadeChromaticAberration(float start, float end, float duration, bool disableAfter = false)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            _chromaticAberration.intensity.value = Mathf.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _chromaticAberration.intensity.value = end;

        if (disableAfter && end == 0f)
            _chromaticAberration.active = false;

        _currentEffect = null;
    }
}