using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

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
    
    [SerializeField] private GameObject empPulsePrefab;
    
    public float repulseRadius = 5f;
    public float repulseForce = 10f;
    
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
        _normalFOV = _mainCamera.fieldOfView;
    }

    void Update()
    {
        MoveForward();
        if (IsMobile())
            RotateWithJoystick();
        else
            RotateToMouse();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            this.UsePowerUp();
        }
        
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
        float targetFOV = _speedActive ? BoostedFOV : _normalFOV;
        _mainCamera.fieldOfView = Mathf.Lerp(
            _mainCamera.fieldOfView,
            targetFOV,
            FOVLerpSpeed * Time.deltaTime
        );
    }
    
    private void MoveForward()
    {
        if (_speedActive)
        {
            transform.position += BaseMoveSpeed * SpeedMultiplier * Time.deltaTime * transform.right;
        }
        else
        {
            transform.position += BaseMoveSpeed * Time.deltaTime * transform.right;
        }
    }
    
    public static bool IsMobile()
    {
        return Application.isMobilePlatform;
    }
    
    private void RotateToMouse()
    {
        // convert screen position to world space
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z); // distance to player
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0f; // to avoid depth drift in 2D

        // calculate direction vector from rocket to mouse
        Vector3 direction = worldPos - transform.position;

        // calculate target rotation as angle
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // smoothly rotate rocket toward target angle
        float angle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    private void RotateWithJoystick()
    {
        Vector2 direction = joystick.inputDirection;
        if (direction.sqrMagnitude < 0.01f)
            return;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
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
                AudioManager.Instance.PlaySound(AudioManager.Instance.pickupSound);
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
    
    private void GameOver()
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

    private void UsePowerUp()
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
