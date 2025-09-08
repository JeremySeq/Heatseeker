using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public GameObject explosionPrefab;
    public Joystick joystick;
    
    [SerializeField] private GameOverUI gameOverUI;
    
    [SerializeField] private static bool _invincible = false;
    
    [SerializeField] private GameObject gameOverCanvas;
    
    [SerializeField] private const float BaseMoveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 10f;
    
    private PowerUpType _heldPowerUp = PowerUpType.None;
    public TextMeshProUGUI powerUpText;
    public TextMeshProUGUI powerUpButtonText;
    public Button powerUpMobileButton;

    public GameObject shieldObject;
    private bool _shieldActive = false;
    private const float ShieldDuration = 5f;
    private float _shieldTimer = 0f;
    
    private bool _speedActive = false;
    private const float SpeedDuration = 5f;
    private float _speedTimer = 0f;
    private const float SpeedMultiplier = 1.5f;
    
    private float _normalFOV;
    private const float BoostedFOV = 70f;
    private const float FOVLerpSpeed = 5f;

    private bool _isGhost = false; // used for phase shift power-up
    private float _ghostTimer = 0f;
    private const float GhostDuration = 5f;
    
    [SerializeField] private GameObject empPulsePrefab;
    
    public float repulseRadius = 5f;
    public float repulseForce = 10f;
    
    private Camera _mainCamera;
    private Rigidbody2D _rb;

    void Start()
    {
        _mainCamera = Camera.main;
        _normalFOV = _mainCamera.fieldOfView;
        powerUpButtonText.text = IsMobile() ? "tap right" : "left click";
        powerUpMobileButton.gameObject.SetActive(IsMobile());

        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (_shieldActive)
        {
            _shieldTimer -= Time.deltaTime;
            if (_shieldTimer <= 0f)
            {
                _shieldActive = false;
                shieldObject.SetActive(false);
            }
        }
        
        if (_speedActive)
        {
            _speedTimer -= Time.deltaTime;
            if (_speedTimer <= 0f)
            {
                _speedActive = false;
            }
        }

        if (this._isGhost)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, .4f, .7f, .7f);
            _ghostTimer -= Time.deltaTime;
            if (_ghostTimer <= 0f)
            {
                _isGhost = false;
                gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                this.GetComponent<Collider2D>().enabled = true;
            }
        }
        
        float targetFOV = _speedActive ? BoostedFOV : _normalFOV;
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFOV, FOVLerpSpeed * Time.deltaTime);

        // powerup use
        if (Mouse.current.leftButton.wasPressedThisFrame)
            UsePowerUp();
    }

    void FixedUpdate()
    {
        MoveForwardPhysics();
        RotatePhysics();
    }

    private void MoveForwardPhysics()
    {
        float speed = _speedActive ? BaseMoveSpeed * SpeedMultiplier : BaseMoveSpeed;
        _rb.linearVelocity = transform.right * speed;
    }
    
    private void RotatePhysics()
    {
        float targetAngle;

        if (IsMobile())
        {
            Vector2 dir = joystick.inputDirection;
            if (dir.sqrMagnitude < 0.01f) return;
            targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        else
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
            Vector2 dir = worldPos - transform.position;
            targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        float angle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        _rb.MoveRotation(angle);
    }

    public static bool IsMobile() => Application.isMobilePlatform;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Shield"))
        {
            return;
        }
        else if (collision.gameObject.CompareTag("PowerUp"))
        {
            this.SetPowerUp(GetRandomPowerUp());
            if (_heldPowerUp != PowerUpType.None)
            {
                AudioManager.Instance.PlaySound(AudioManager.Instance.pickupSound, .8f);
            }
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Missile") && _shieldActive)
        {
            // shouldn't happen, because shield collider should destroy missiles first, but just in case
            collision.gameObject.GetComponent<MissileController>().Explode();
        }
        else
        {
            GameOver();
        }
    }
    
    PowerUpType GetRandomPowerUp()
    {
        int powerUpCount = System.Enum.GetValues(typeof(PowerUpType)).Length;
        // exclude PowerUpType.None (index 0)
        return (PowerUpType) Random.Range(1, powerUpCount);
    }
    
    public void GameOver()
    {
        if (_invincible)
        {
            return;
        }
        
        Camera.main.GetComponent<CameraFollow>().Shake(.5f, 3);
        PostProcessingFX.FadeInChromaticAberration(1f, .5f, false);
        
        ScoreUI.Instance.StopTimer();
        
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        // disable player
        gameObject.SetActive(false);
        gameOverUI.Show();
        Debug.Log("GAME OVER");
        Destroy(gameObject);
    }
    
    private void SetPowerUp(PowerUpType newPowerUp)
    {
        _heldPowerUp = newPowerUp;
        powerUpText.text = _heldPowerUp.ToString();
        if (_heldPowerUp != PowerUpType.None)
        {
            powerUpText.color = new Color(253/255f, 131/255f, 54/255f, 1f);
        }
        else
        {
            powerUpText.color = Color.white;
        }
    }

    public void UsePowerUp()
    {

        switch (_heldPowerUp)
        {
            case PowerUpType.Shield:
                _shieldActive = true;
                shieldObject.SetActive(true);
                _shieldTimer = ShieldDuration;
                AudioManager.Instance.PlaySound(AudioManager.Instance.shieldSound);
                break;
            case PowerUpType.Speed:
                _speedActive = true;
                _speedTimer = SpeedDuration;
                AudioManager.Instance.PlaySound(AudioManager.Instance.speedBoostSound);
                break;
            case PowerUpType.EMP:
                DoubleShockwave();
                break;
            case PowerUpType.Repulsor:
                Repulse();
                break;
			case PowerUpType.PhaseShift:
                this._isGhost = true;
                this._ghostTimer = GhostDuration;
                this.GetComponent<Collider2D>().enabled = false;
                PostProcessingFX.PulseChromaticAberration(holdTime: 5f);
                AudioManager.Instance.PlaySound(AudioManager.Instance.phaseShiftSound);
            	break;
        }
    
        SetPowerUp(PowerUpType.None);
    }
    
    private void DoubleShockwave()
    {
        Camera.main.GetComponent<CameraFollow>().Shake(1, 2);

        GameObject pulse1 = Instantiate(empPulsePrefab, transform.position, Quaternion.identity, transform);
        pulse1.GetComponent<EMPPulse>().duration = 1f;

        GameObject pulse2 = Instantiate(empPulsePrefab, transform.position, Quaternion.identity, transform);
        pulse2.GetComponent<EMPPulse>().duration = 2f;
        pulse2.GetComponent<LineRenderer>().startWidth *= 3;
        pulse2.GetComponent<EMPPulse>().pulseColor = new Color(1, 1, 1, .5f);
        pulse2.GetComponent<EMPPulse>().disableMissiles = false;

        PostProcessingFX.PulseChromaticAberration(
            fadeInTime: 0.1f,
            holdTime: 1.5f,
            fadeOutTime: 0.5f,
            maxIntensity: 1f
        );
    }

    private void Repulse()
    {
        Vector2 center = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, repulseRadius);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            MissileController missile = hit.GetComponent<MissileController>();
            if (missile != null)
            {
                Vector2 direction = (hit.transform.position - transform.position).normalized;
                missile.Repulse(direction * repulseForce);
            }
            if (hit.attachedRigidbody != null && hit.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 direction = (hit.transform.position - transform.position).normalized;
                float mass = hit.attachedRigidbody.mass;
                hit.attachedRigidbody.AddForce(repulseForce * mass * direction, ForceMode2D.Impulse);
            }
        }
        AudioManager.Instance.PlaySound(AudioManager.Instance.repulsorSound);
        PostProcessingFX.PulseChromaticAberration(0.1f, 0.1f, 0.2f, 0.7f);
        Camera.main.GetComponent<CameraFollow>()?.Shake(0.5f, 0.3f);
    }
}
