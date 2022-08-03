using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, Controls.IPlayerActions
{
    // Player Properties
    [Header("Player")]
    [SerializeField] private int _maxLives = 3;
    private int _score;
    private int _lives;
    private float _height;
    private float _width;
    
    // Movement Properties
    [Header("Base Movement")]
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField]private float _verticalStartPosition = -2.0f;
    [SerializeField]private float _horizontalStartPosition = 0f;
    private Vector2 _direction;
    
    // Play Space Boundaries Properties
    private ViewportBounds _viewportBounds;
    [SerializeField] private float _playerMovementLimitFromTop = 0;
    [SerializeField] private float _playerMovementLimitFromBottom = 0;
    private float _movementLimitTop;
    private float _movementLimitBottom;
    private float _zPos = 0f;

    // Projectile Properties
    [Header("Weapons")]
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private int _maxAmmoCount = 15;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private GameObject _tripleShotPrefab;
    [SerializeField] private GameObject _sprayShotPrefab;
    [SerializeField] private GameObject _homingMissilePrefab;
    private bool _tripleShotActive;
    private bool _sprayShotActive;
    private bool _homingMissileActive;
    private float _nextFire = -1f;
    private int _ammoCount;
    
    // Powerup Collection
    private bool _powerupCollectionActive;
    
    // Thrusters Properties
    [Header("Thrusters")]
    [SerializeField] private int _maxThrusterCharge = 100;
    [SerializeField] private int _thrusterUseRate = 1;
    [SerializeField] private int _thrusterRechargeRate = 1;
    [SerializeField] private float _thrusterCooldownTime = 2.0f;
    private int _thrusterCharge;
    private bool _thrusterActived;
    private bool _thrusterActive;
    private bool _thrusterRecharging;
    
    // Speed Boost Properties
    [Header("Speed Boost")]
    [SerializeField] private float _speedBoostModifier = 2.0f;
    private bool _speedBoostActive;
    
    // Shield Properties
    [Header("Shields")]
    [SerializeField] private int _maxShieldStrength = 3;
    private bool _shieldsActive;
    private int _shieldStrength;
    
    // Animations / Visualizers
    private Animator _playerAnimator;
    private GameObject _shieldVisualizer;
    private GameObject _rightEngineDamageVisualizer;
    private GameObject _leftEngineDamageVisualizer;
    private GameObject _thrusterVisualizer;
    private static readonly int IsTurningLeft = Animator.StringToHash("isTurningLeft");
    private static readonly int IsTurningRight = Animator.StringToHash("isTurningRight");
    private Renderer _renderer;
    private BoxCollider2D _collider;
    
    // Audio
    [Header("Audio")]
    [SerializeField] private AudioClip _laserSound;
    [SerializeField] private AudioClip _powerupSound;
    [SerializeField] private AudioClip _explosionSound;
    [SerializeField] private AudioClip _ammoDepletedSound;
    private AudioSource _audioSource;
    
    // Player Input
    [Header("Input")]
    [SerializeField] private PlayerInput _playerInput;

    private Powerup[] _powerupsOnScreen = { };

    void Start()
    {
        _lives = _maxLives;
        _ammoCount = _maxAmmoCount;
        _thrusterCharge = _maxThrusterCharge;

        _renderer = GetComponent<Renderer>();
        if (_renderer == null) { Debug.LogError("Renderer in Player class is NULL"); }
        
        _collider = GetComponent<BoxCollider2D>();
        if (_collider == null) { Debug.LogError("Collider in Player class is NULL"); }

        _playerAnimator = GetComponent<Animator>();
        if (_playerAnimator == null) { Debug.LogError("Animator in Player class is NULL"); }
        
        if (GameManager.Instance == null) { Debug.LogError("Game Manager in Player class is NULL"); }
        
        _shieldVisualizer = transform.Find("Shield_Visualizer")?.gameObject;
        if (_shieldVisualizer == null) { Debug.LogError("Shield Visualizer in Player class is NULL"); }
        else { _shieldVisualizer.SetActive(false); }
        
        _rightEngineDamageVisualizer = transform.Find("Right_Engine_Damage_Visualizer")?.gameObject;
        if (_rightEngineDamageVisualizer == null) { Debug.LogError("Right Engine Damage Visualizer in Player class is NULL"); }
        
        _leftEngineDamageVisualizer = transform.Find("Left_Engine_Damage_Visualizer")?.gameObject;
        if (_leftEngineDamageVisualizer == null) { Debug.LogError("Left Engine Damage Visualizer in Player class is NULL"); }
        
        UpdateEngineDamageVisualizers(_lives);
        
        _thrusterVisualizer = transform.Find("Thruster")?.gameObject;
        if (_thrusterVisualizer == null) { Debug.LogError("Shield Visualizer in Player class is NULL"); }
        
        if (_laserPrefab == null) { Debug.LogError("Laser Prefab in Player class is NULL"); }
        if (_tripleShotPrefab == null) { Debug.LogError("Triple Shot Prefab in Player class is NULL"); }
        if (_sprayShotPrefab == null) { Debug.LogError("Spray Shot Prefab in Player class is NULL"); }
        if (_homingMissilePrefab == null) { Debug.LogError("Homing Missile Prefab in Player class is NULL"); }
        
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) { Debug.LogError("AudioSource in Player class is NULL"); }
        
        if (_laserSound == null) { Debug.LogError("Laser Sound missing from Player!"); }
        if (_explosionSound == null) { Debug.LogError("Explosion Sound missing from Player!"); }
        if (_powerupSound == null) { Debug.LogError("Powerup Sound missing from Player!"); }
        if (_ammoDepletedSound == null) { Debug.LogError("Ammo Depleted Sound missing from Player!"); }
        
        transform.position = new Vector3(_horizontalStartPosition, _verticalStartPosition, _zPos);
        _score = 0;
        if (GameManager.Instance == null)
        {
            Debug.LogError("Game Manager is NULL");
        }
        else
        {
            _viewportBounds = GameManager.Instance.GetViewportBounds();
            if (_viewportBounds == null)
            {
                Debug.LogError("Viewport Bounds is NULL on Player!");
            }
            
            GameManager.Instance.SetScore(_score);
            GameManager.Instance.SetLives(_lives);
            GameManager.Instance.UpdateAmmoCount(_ammoCount, _maxAmmoCount);
            GameManager.Instance.UpdateMaxThrusterCharge(_maxThrusterCharge);
            GameManager.Instance.UpdateThrusterCharge(_thrusterCharge);
            
            var playerDimensions = transform.localScale;
            _width = playerDimensions.x;
            _height = playerDimensions.y;

            _movementLimitBottom = _viewportBounds.Bottom + _playerMovementLimitFromBottom + _height / 2;
            _movementLimitTop = _viewportBounds.Top - _playerMovementLimitFromTop - _height / 2;
        }
    }

    void Update()
    {
        CalculateMovement();
        CollectPowerupsOnScreen();
    }

    private void CollectPowerupsOnScreen()
    {
        if (_powerupCollectionActive && _powerupsOnScreen.Any())
        {
            _powerupsOnScreen.ToList().ForEach(p =>
            {
                if (p != null)
                {
                    p.MoveTowardsPosition(transform.position);
                }
            });
        }
    }

    private void CalculateMovement()
    {
        _thrusterActive = _thrusterActived && _thrusterCharge > 0;
        _playerAnimator.SetBool(IsTurningLeft, _direction.x < 0);
        _playerAnimator.SetBool(IsTurningRight, _direction.x > 0);

        if (_thrusterActive)
        {
            if (_thrusterRecharging)
            {
                StopCoroutine(nameof(ThrusterRechargeRoutine));
                _thrusterRecharging = false;
            }

            
            _thrusterCharge -= _thrusterCharge > 0 ? _thrusterUseRate : 0;
            GameManager.Instance.UpdateThrusterCharge(_thrusterCharge);
            //TODO: Refactor Hacky Magic Numbers in local transforms below
            _thrusterVisualizer.transform.localPosition = new Vector3(0, -3.1f, 0);
            _thrusterVisualizer.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        } else
        {
            //TODO: Refactor Hacky Magic Numbers in local transforms below
            _thrusterVisualizer.transform.localPosition = new Vector3(0, -2f, 0);
            _thrusterVisualizer.transform.localScale = new Vector3(0.25f, 0.25f, 1.0f);
            if (!_thrusterRecharging && _thrusterCharge < _maxThrusterCharge)
            {
                StartCoroutine(nameof(ThrusterRechargeRoutine));
            }
        }

        float modifiedSpeed = _speedBoostActive || _thrusterActive 
            ? _movementSpeed * _speedBoostModifier 
            : _movementSpeed;
        transform.Translate(_direction * (modifiedSpeed * Time.deltaTime));
        float yPosClamped = Mathf.Clamp(transform.position.y, _movementLimitBottom, _movementLimitTop);
        
        float xPos = transform.position.x;
        if (xPos < _viewportBounds.Left)
        {
            xPos = _viewportBounds.Right;
        }
        else if (xPos > _viewportBounds.Right)
        {
            xPos = _viewportBounds.Left;
        }
        
        transform.position = new Vector3(xPos, yPosClamped, _zPos);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _direction = context.ReadValue<Vector2>().normalized;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started && Time.time > _nextFire)
        {
            if (_ammoCount > 0)
            {
                _ammoCount--;
                GameManager.Instance.UpdateAmmoCount(_ammoCount, _maxAmmoCount);
                _nextFire = Time.time + _fireRate;
                GameObject shotPrefab;
                
                if (_tripleShotActive)
                {
                    shotPrefab = _tripleShotPrefab;
                }
                else if (_sprayShotActive)
                {
                    shotPrefab = _sprayShotPrefab;
                } else if (_homingMissileActive)
                {
                    shotPrefab = _homingMissilePrefab;
                }
                else
                {
                    shotPrefab = _laserPrefab;
                }
                
                Instantiate(shotPrefab, transform.position, Quaternion.identity);
                if (_audioSource != null) { _audioSource.PlayOneShot(_laserSound); }
            }
            else
            {
                _audioSource.PlayOneShot(_ammoDepletedSound);
            }
        }
    }

    public void OnSpeedBoost(InputAction.CallbackContext context)
    {
        if (context.performed) { _thrusterActived = true; }
        if (context.canceled) { _thrusterActived = false; }
    }

    public void OnPowerupCollect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _powerupsOnScreen = FindObjectsOfType<Powerup>();
            _powerupCollectionActive = true;
        }

        if (context.canceled)
        {
            _powerupCollectionActive = false;
            _powerupsOnScreen.ToList().ForEach(p => p.ResumeDefaultMovement());
        }
    }

    public void Damage()
    {
        if (_shieldsActive)
        {
            _shieldStrength--;
            SetShieldVisualizerColor(_shieldVisualizer, _shieldStrength);
            GameManager.Instance.UpdateShieldStrength(_shieldStrength);
            _shieldsActive = _shieldStrength > 0;
            if (_shieldVisualizer != null) { _shieldVisualizer.SetActive(_shieldsActive); }
            _audioSource.PlayOneShot(_explosionSound);
        }
        else
        {
            _lives--;
            GameManager.Instance.SetLives(_lives);
            GameManager.Instance.ShakeCamera();
            _audioSource.PlayOneShot(_explosionSound);
            UpdateEngineDamageVisualizers(_lives);
            if (_lives <= 0)
            {
                _collider.enabled = false;
                _renderer.enabled = false;
                _thrusterVisualizer.SetActive(false);
                _playerInput.SwitchCurrentActionMap("UI");
            }
        }
    }

    private void SetShieldVisualizerColor(GameObject shieldVisualizer, int shieldStrength)
    {
        switch (shieldStrength)
        {
            case 3:
                shieldVisualizer.GetComponent<SpriteRenderer>().color = Color.white;
                break;
            case 2:
                shieldVisualizer.GetComponent<SpriteRenderer>().color = Color.green;
                break;
            case 1:
                shieldVisualizer.GetComponent<SpriteRenderer>().color = Color.red;
                break;
            case 0:
                shieldVisualizer.GetComponent<SpriteRenderer>().color = new Color(0,0,0,0);
                break;
            default:
                Debug.LogError("_shieldStrength value out of range");
                break;
        }
    }
    
    public void ActivatePowerup(Powerup powerup)
    {
        var type = powerup.GetPowerupType();
        switch (type)
        {
            case PowerupType.TripleShot:
                if (_homingMissileActive) { StopCoroutine(nameof(HomingMissileCooldownRoutine)); }
                _homingMissileActive = false;
                if (_sprayShotActive) { StopCoroutine(nameof(SprayShotCooldownRoutine)); }
                _sprayShotActive = false;
                if (_tripleShotActive) { StopCoroutine(nameof(TripleShotCooldownRoutine)); }
                _tripleShotActive = true;
                _audioSource.PlayOneShot(_powerupSound);
                StartCoroutine(nameof(TripleShotCooldownRoutine), powerup.GetEffectDuration());
                break;
            case PowerupType.SpeedBoost:
                if (_speedBoostActive) { StopCoroutine(nameof(SpeedBoostCooldownRoutine)); }
                _speedBoostActive = true;
                _audioSource.PlayOneShot(_powerupSound);
                StartCoroutine(nameof(SpeedBoostCooldownRoutine), powerup.GetEffectDuration());
                break;
            case PowerupType.Shields:
                _shieldStrength = _maxShieldStrength;
                SetShieldVisualizerColor(_shieldVisualizer, _shieldStrength);
                GameManager.Instance.UpdateShieldStrength(_shieldStrength);
                _shieldsActive = _shieldStrength > 0;
                if (_shieldVisualizer != null) { _shieldVisualizer.SetActive(_shieldsActive); }
                _audioSource.PlayOneShot(_powerupSound);
                break;
            case PowerupType.Ammo:
                _ammoCount = _maxAmmoCount;
                GameManager.Instance.UpdateAmmoCount(_ammoCount, _maxAmmoCount);
                _audioSource.PlayOneShot(_powerupSound);
                break;
            case PowerupType.Health:
                if (_lives > 0 && _lives < _maxLives) { _lives++;  }
                GameManager.Instance.SetLives(_lives);
                UpdateEngineDamageVisualizers(_lives);
                _audioSource.PlayOneShot(_powerupSound);
                Debug.Log("Health Powerup Collected");
                break;
            case PowerupType.SprayShot:
                if (_homingMissileActive) { StopCoroutine(nameof(HomingMissileCooldownRoutine)); }
                _homingMissileActive = false;
                if (_tripleShotActive) { StopCoroutine(nameof(TripleShotCooldownRoutine)); }
                _tripleShotActive = false;
                if (_sprayShotActive) { StopCoroutine(nameof(SprayShotCooldownRoutine)); }
                _sprayShotActive = true;
                _audioSource.PlayOneShot(_powerupSound);
                StartCoroutine(nameof(SprayShotCooldownRoutine), powerup.GetEffectDuration());
                break;
            case PowerupType.Hazard:
                Damage();
                break;
            case PowerupType.HomingMissile:
                if (_tripleShotActive) { StopCoroutine(nameof(TripleShotCooldownRoutine)); }
                _tripleShotActive = false;
                if (_sprayShotActive) { StopCoroutine(nameof(SprayShotCooldownRoutine)); }
                _sprayShotActive = false;
                if (_homingMissileActive) { StopCoroutine(nameof(HomingMissileCooldownRoutine)); }
                _homingMissileActive = true;
                _audioSource.PlayOneShot(_powerupSound);
                StartCoroutine(nameof(SprayShotCooldownRoutine), powerup.GetEffectDuration());
                break;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void IncreaseScore(int value)
    {
        _score += value;
        GameManager.Instance.SetScore(_score);
    }
    
    private void UpdateEngineDamageVisualizers(int lives)
    {
        switch (lives)
        {
           case 2:
                _leftEngineDamageVisualizer.SetActive(true);
                _rightEngineDamageVisualizer.SetActive(false);
                break;
            case 1:
                _leftEngineDamageVisualizer.SetActive(true);
                _rightEngineDamageVisualizer.SetActive(true);
                break;
            default:
                // cases for 3 or 0 lives are both handled here
                _leftEngineDamageVisualizer.SetActive(false);
                _rightEngineDamageVisualizer.SetActive(false);
                break;
        }
    }

    private void RechargeThrusters()
    {
        if (_thrusterCharge < _maxThrusterCharge)
        {
            _thrusterCharge += _thrusterRechargeRate;
        }
        else
        {
            _thrusterCharge = _maxThrusterCharge;
        }

        GameManager.Instance.UpdateThrusterCharge(_thrusterCharge);
    }
    
    IEnumerator TripleShotCooldownRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        _tripleShotActive = false;
    }
    
    IEnumerator HomingMissileCooldownRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        _homingMissileActive = false;
    }
    
    IEnumerator SpeedBoostCooldownRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        _speedBoostActive = false;
    }

    IEnumerator SprayShotCooldownRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        _sprayShotActive = false;
    }
    
    
    IEnumerator ThrusterRechargeRoutine()
    {
        _thrusterRecharging = true;
        yield return new WaitForSecondsRealtime(_thrusterCooldownTime);
        while (_thrusterCharge < _maxThrusterCharge)
        {
            yield return new WaitForFixedUpdate();
            RechargeThrusters();
        }

        _thrusterRecharging = false;
    }
}
