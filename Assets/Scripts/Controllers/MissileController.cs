using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
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
    private float _repulseDecayRate = 4f; // how quickly the repulse fades
    private Rigidbody2D _rb;

    private float _moveSpeed;
    private float _rotationSpeed;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _moveSpeed = moveSpeed + (Random.value * .4f) - 0.2f; // Randomize speed slightly
        _rotationSpeed = rotationSpeed + (Random.value * .4f) - 0.2f; // Randomize rotation speed slightly
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _target = playerObj.transform;
        }
    }

    private void Update()
    {
        ApplyExternalVelocity();
    }

    void FixedUpdate()
    {
        if (_trackingEnabled && _target != null)
        {
            RotateToTarget();
        }
        else
        {
            _disableTimer -= Time.fixedDeltaTime;
            if (_disableTimer <= 0f)
                _trackingEnabled = true;
        }

        Vector2 forwardVelocity = transform.right * _moveSpeed;
        _rb.linearVelocity = forwardVelocity + _externalVelocity;
    }

    
    private void RotateToTarget()
    {
        Vector3 targetPos = _target.position;
        targetPos.z = 0f;

        Vector3 direction = targetPos - transform.position;

        // calculate target rotation as angle
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // smoothly rotate rocket toward target angle
        float angle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, _rotationSpeed * Time.fixedDeltaTime);

        _rb.MoveRotation(angle);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D targetRb = collision.rigidbody;

        if (targetRb != null && !collision.gameObject.CompareTag("Missile"))
        {
            Vector2 impactDir = collision.relativeVelocity.normalized;

            targetRb.AddForce(impactDir * impactForce, ForceMode2D.Impulse);
            Explode();
        }
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
            _rb.linearVelocity += _externalVelocity;
            _externalVelocity = Vector2.Lerp(_externalVelocity, Vector2.zero, _repulseDecayRate * Time.deltaTime);
        }
    }
    
    public void Repulse(Vector2 force)
    {
        force.Scale(new Vector2(3f, 3f));
        _externalVelocity += force;
    }
}
