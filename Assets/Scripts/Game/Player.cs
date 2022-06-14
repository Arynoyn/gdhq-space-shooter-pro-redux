using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, Controls.IPlayerActions
{
    // Player Properties
    [Header("Player")]
    [SerializeField] private int _lives = 3;
    [SerializeField] private int _score;
    
    // Game State Managers
    [Header("Managers")]
    private GameManager _gameManager;
    
    // Movement Properties
    [Header("Movement")]
    [SerializeField] private float _movementSpeed = 5f;
    private bool _thrusterActive = false;
    private Vector2 _direction;
    private float _verticalStartPosition = -2.0f;
    private float _horizontalStartPosition = 0f;
    private float _topMovementLimit = 0f;
    private float _bottomMovementLimit = -3.8f;
    private float _leftMovementLimit = -9.5f;
    private float _rightMovementLimit = 9.5f;
    private float _zPos = 0f;

    // Projectile Properties
    [Header("Weapons")]
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private int _maxAmmoCount = 15;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private GameObject _tripleShotPrefab;
    private bool _tripleShotActive;
    private Vector3 _laserOffset = new Vector3(0f, 1.25f, 0f);
    private float _nextFire = -1f;
    private int _ammoCount = 0;
    
    // Speed Boost Properties
    [Header("Speed")]
    [SerializeField] private float _speedBoostModifier = 2.0f;
    private bool _speedBoostActive;
    
    // Shield Properties
    [Header("Shields")]
    [SerializeField] private int _maxShieldStrength = 3;
    private bool _shieldsActive;
    private int _shieldStrength;
    
    // Animations / Visualizers
    [SerializeField] private Animator _playerAnimator;
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

    void Start()
    {
        _ammoCount = _maxAmmoCount;

        _renderer = GetComponent<Renderer>();
        if (_renderer == null) { Debug.LogError("Renderer in Player class is NULL"); }
        
        _collider = GetComponent<BoxCollider2D>();
        if (_collider == null) { Debug.LogError("Collider in Player class is NULL"); }

        _playerAnimator = GetComponent<Animator>();
        if (_playerAnimator == null) { Debug.LogError("Animator in Player class is NULL"); }
        
        _gameManager = FindObjectOfType<GameManager>();
        if (_gameManager == null) { Debug.LogError("Game Manager in Player class is NULL"); }
        
        _shieldVisualizer = transform.Find("Shield_Visualizer")?.gameObject;
        if (_shieldVisualizer == null) { Debug.LogError("Shield Visualizer in Player class is NULL"); }
        else { _shieldVisualizer.SetActive(false); }
        
        _rightEngineDamageVisualizer = transform.Find("Right_Engine_Damage_Visualizer")?.gameObject;
        if (_rightEngineDamageVisualizer == null) { Debug.LogError("Right Engine Damage Visualizer in Player class is NULL"); }
        else { _rightEngineDamageVisualizer.SetActive(false); }
        
        _leftEngineDamageVisualizer = transform.Find("Left_Engine_Damage_Visualizer")?.gameObject;
        if (_leftEngineDamageVisualizer == null) { Debug.LogError("Left Engine Damage Visualizer in Player class is NULL"); }
        else { _leftEngineDamageVisualizer.SetActive(false); }
        
        _thrusterVisualizer = transform.Find("Thruster")?.gameObject;
        if (_thrusterVisualizer == null) { Debug.LogError("Shield Visualizer in Player class is NULL"); }
        
        if (_laserPrefab == null) { Debug.LogError("Laser Prefab in Player class is NULL"); }
        if (_tripleShotPrefab == null) { Debug.LogError("Triple Shot Prefab in Player class is NULL"); }
        
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) { Debug.LogError("AudioSource in Player class is NULL"); }
        
        if (_laserSound == null) { Debug.LogError("Laser Sound missing from Player!"); }
        if (_explosionSound == null) { Debug.LogError("Explosion Sound missing from Player!"); }
        if (_powerupSound == null) { Debug.LogError("Powerup Sound missing from Player!"); }
        if (_ammoDepletedSound == null) { Debug.LogError("Ammo Depleted Sound missing from Player!"); }
        
        transform.position = new Vector3(_horizontalStartPosition, _verticalStartPosition, _zPos);
        _score = 0;
        _gameManager.SetScore(_score);
        _gameManager.SetLives(_lives);
        _gameManager.UpdateAmmoCount(_ammoCount);
    }

    void Update()
    {       
        CalculateMovement();
    }

    private void CalculateMovement()
    {
        float modifiedSpeed = _speedBoostActive || _thrusterActive 
            ? _movementSpeed * _speedBoostModifier 
            : _movementSpeed;
        _playerAnimator.SetBool(IsTurningLeft, _direction.x < 0);
        _playerAnimator.SetBool(IsTurningRight, _direction.x > 0);
        transform.Translate(_direction * (modifiedSpeed * Time.deltaTime));
        float yPosClamped = Mathf.Clamp(transform.position.y, _bottomMovementLimit, _topMovementLimit);
        
        float xPos = transform.position.x;
        if (xPos < _leftMovementLimit)
        {
            xPos = _rightMovementLimit;
        }
        else if (xPos > _rightMovementLimit)
        {
            xPos = _leftMovementLimit;
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
                _gameManager.UpdateAmmoCount(_ammoCount);
                _nextFire = Time.time + _fireRate;
                Vector3 playerPosition = transform.position;
                Vector3 laserSpawnOffset = _tripleShotActive
                    ? playerPosition
                    : playerPosition + _laserOffset;
                GameObject shotPrefab = _tripleShotActive
                    ? _tripleShotPrefab
                    : _laserPrefab;
                Instantiate(shotPrefab, laserSpawnOffset, Quaternion.identity);
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
        if (context.performed)
        {
            _thrusterActive = !_thrusterActive;
        }

        if (context.canceled)
        {
            _thrusterActive = false;
        }
    }

    public void Damage()
    {
        if (_shieldsActive)
        {
            _shieldStrength--;
            SetShieldVisualizerColor(_shieldVisualizer, _shieldStrength);
            _gameManager.UpdateShieldStrength(_shieldStrength);
            _shieldsActive = _shieldStrength > 0;
            if (_shieldVisualizer != null) { _shieldVisualizer.SetActive(_shieldsActive); }
            _audioSource.PlayOneShot(_explosionSound);
        }
        else
        {
            _lives--;
            _gameManager.SetLives(_lives);
            _audioSource.PlayOneShot(_explosionSound);
            if (_lives < 1)
            {
                _collider.enabled = false;
                _renderer.enabled = false;
                _leftEngineDamageVisualizer.SetActive(false);
                _rightEngineDamageVisualizer.SetActive(false);
                _thrusterVisualizer.SetActive(false);
                _playerInput.SwitchCurrentActionMap("UI");
            }
            else
            {
                UpdateEngineDamageVisualizers(_lives);
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
    
    public void ActivatePowerup(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.TripleShot:
                if (_tripleShotActive) { StopCoroutine(nameof(TripleShotCooldownRoutine)); }
                _tripleShotActive = true;
                _audioSource.PlayOneShot(_powerupSound);
                StartCoroutine(nameof(TripleShotCooldownRoutine));
                break;
            case PowerupType.SpeedBoost:
                if (_speedBoostActive) { StopCoroutine(nameof(SpeedBoostCooldownRoutine)); }
                _speedBoostActive = true;
                _audioSource.PlayOneShot(_powerupSound);
                StartCoroutine(nameof(SpeedBoostCooldownRoutine));
                break;
            case PowerupType.Shields:
                _shieldStrength = _maxShieldStrength;
                SetShieldVisualizerColor(_shieldVisualizer, _shieldStrength);
                _gameManager.UpdateShieldStrength(_shieldStrength);
                _shieldsActive = _shieldStrength > 0;
                if (_shieldVisualizer != null) { _shieldVisualizer.SetActive(_shieldsActive); }
                _audioSource.PlayOneShot(_powerupSound);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void IncreaseScore(int value)
    {
        _score += value;
        _gameManager.SetScore(_score);
    }
    
    private void UpdateEngineDamageVisualizers(int lives)
    {
        switch (lives)
        {
            case 3:
                _leftEngineDamageVisualizer.SetActive(false);
                _rightEngineDamageVisualizer.SetActive(false);
                break;
            case 2:
                _leftEngineDamageVisualizer.SetActive(true);
                _rightEngineDamageVisualizer.SetActive(false);
                break;
            case 1:
                _leftEngineDamageVisualizer.SetActive(true);
                _rightEngineDamageVisualizer.SetActive(true);
                break;
            default:
                break;
        }
    }
    
    IEnumerator TripleShotCooldownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _tripleShotActive = false;
    }
    
    IEnumerator SpeedBoostCooldownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _speedBoostActive = false;
    }
}
