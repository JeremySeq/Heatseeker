using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private float smoothSpeed = 5f;

    private Vector3 _initialOffset;
    private float _shakeDuration = 0f;
    private float _shakeMagnitude = 0f;
    private Vector3 _shakeOffset;

    private void Awake()
    {
        _initialOffset = offset;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // shake logic
        if (_shakeDuration > 0)
        {
            _shakeOffset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f) * _shakeMagnitude,
                UnityEngine.Random.Range(-1f, 1f) * _shakeMagnitude,
                0f
            );
            _shakeDuration -= Time.deltaTime;
        }
        else
        {
            _shakeOffset = Vector3.zero;
        }

        // follow + shake
        Vector3 desiredPosition = target.position + offset + _shakeOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    // call this to shake the camera
    public void Shake(float duration, float magnitude)
    {
        _shakeDuration = duration;
        _shakeMagnitude = magnitude;
    }
}
