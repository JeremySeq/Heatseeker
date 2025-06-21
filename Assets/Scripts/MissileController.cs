using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MissileController : MonoBehaviour
{
    
    public GameObject explosionPrefab;
    
    private Transform _target;
    private bool _trackingEnabled = true;
    private float _disableTimer = 0f;
    [SerializeField] private static float moveSpeed = 7.5f;
    [SerializeField] private static float rotationSpeed = 4.2f;
    [SerializeField] private static float impactForce = 2f;
    
    private Vector2 _externalVelocity; // velocity from repulses
    private float _repulseDecayRate = 5f; // how quickly the repulse fades

    private float _moveSpeed;
    private float _rotationSpeed;

    private void Start()
    {
        _moveSpeed = moveSpeed + (Random.value * .4f) - 0.2f; // Randomize speed slightly
        _rotationSpeed = rotationSpeed + (Random.value * .4f) - 0.2f; // Randomize rotation speed slightly
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        _target = playerObj.transform;
    }

    void Update()
    {
        ApplyExternalVelocity();
        MoveForward();
        if (!_trackingEnabled)
        {
            _disableTimer -= Time.deltaTime;
            if (_disableTimer <= 0f)
            {
                _trackingEnabled = true;
            }
        }
        else
        {
            RotateToTarget();
        }
    }
    
    private void MoveForward()
    {
        transform.position += _moveSpeed * Time.deltaTime * transform.right;
    }
    
    private void RotateToTarget()
    {
        if (_target == null)
        {
            return; // target not set, do nothing
        }
        
        Vector3 targetPos = _target.position;
        targetPos.z = 0f;

        Vector3 direction = targetPos - transform.position;

        // calculate target rotation as angle
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // smoothly rotate rocket toward target angle
        float angle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, _rotationSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D targetRb = collision.rigidbody;

        if (targetRb != null)
        {
            Vector2 impactDir = collision.relativeVelocity.normalized;

            targetRb.AddForce(impactDir * impactForce, ForceMode2D.Impulse);
        }

        Explode();
    }

    public void DisableTrackingTemporarily(float duration)
    {
        _trackingEnabled = false;
        _disableTimer = duration;
    }
    
    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    
    private void ApplyExternalVelocity()
    {
        if (_externalVelocity.sqrMagnitude > 0.001f)
        {
            transform.position += (Vector3)(_externalVelocity * Time.deltaTime);
            _externalVelocity = Vector2.Lerp(_externalVelocity, Vector2.zero, _repulseDecayRate * Time.deltaTime);
        }
    }
    
    public void Repulse(Vector2 force)
    {
        _externalVelocity += force;
    }
}
