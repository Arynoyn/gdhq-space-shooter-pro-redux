using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, Controls.IPlayerActions
{
    //Player Properties
    [SerializeField] private int _lives = 3;
    [SerializeField] private int _score;
    
    // Game State Managers
    private GameManager _gameManager;
    
    // Movement Properties
    [SerializeField] private float _movementSpeed = 5f;
    private Vector2 _direction;
    private float _verticalStartPosition = -2.0f;
    private float _horizontalStartPosition = 0f;
    private float _topMovementLimit = 0f;
    private float _bottomMovementLimit = -3.8f;
    private float _leftMovementLimit = -9.5f;
    private float _rightMovementLimit = 9.5f;
    private float _zPos = 0f;

    //Projectile Properties
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private GameObject _tripleShotPrefab;
    private bool _tripleShotActive;
    private Vector3 _laserOffset = new Vector3(0f, 1.25f, 0f);
    private float _nextFire = -1f;
    
    //Speed Boost Properties
    [SerializeField] private float _speedBoostModifier = 2.0f;
    private bool _speedBoostActive;
    
    //Shield Properties
    [SerializeField] private int _maxShieldStrength = 3;
    private bool _shieldsActive;
    private int _shieldStrength;
    
    // Animations / Visualizers
    [SerializeField] private Animator _playerAnimator;
    private GameObject _shieldVisualizer;
    private GameObject _rightEngineDamageVisualizer;
    private GameObject _leftEngineDamageVisualizer;
    private static readonly int IsTurningLeft = Animator.StringToHash("isTurningLeft");
    private static readonly int IsTurningRight = Animator.StringToHash("isTurningRight");

    void Start()
    {
        _playerAnimator = GetComponent<Animator>();
        if (_playerAnimator == null)
        {
            Debug.LogError("Animator in Player class is NULL");
        }
        
        _gameManager = FindObjectOfType<GameManager>();
        if (_gameManager == null)
        {
            Debug.LogError("Game Manager in Player class is NULL");
        }
        
        _shieldVisualizer = transform.Find("Shield_Visualizer")?.gameObject;
        if (_shieldVisualizer == null)
        {
            Debug.LogError("Shield Visualizer in Player class is NULL");
        }
        else
        {
            _shieldVisualizer.SetActive(false);
        }
        
        _rightEngineDamageVisualizer = transform.Find("Right_Engine_Damage_Visualizer")?.gameObject;
        if (_rightEngineDamageVisualizer == null)
        {
            Debug.LogError("Right Engine Damage Visualizer in Player class is NULL");
        }
        else
        {
            _rightEngineDamageVisualizer.SetActive(false);
        }
        
        _leftEngineDamageVisualizer = transform.Find("Left_Engine_Damage_Visualizer")?.gameObject;
        if (_leftEngineDamageVisualizer == null)
        {
            Debug.LogError("Left Engine Damage Visualizer in Player class is NULL");
        }
        else
        {
            _leftEngineDamageVisualizer.SetActive(false);
        }
        
        if (_laserPrefab == null)
        {
            Debug.LogError("Laser Prefab in Player class is NULL");
        }
        
        if (_tripleShotPrefab == null)
        {
            Debug.LogError("Triple Shot Prefab in Player class is NULL");
        }
        
        transform.position = new Vector3(_horizontalStartPosition, _verticalStartPosition, _zPos);
        _score = 0;
        _gameManager.SetScore(_score);
        _gameManager.SetLives(_lives);
    }

    void Update()
    {       
        CalculateMovement();
    }

    private void CalculateMovement()
    {
        float modifiedSpeed = _speedBoostActive ? _movementSpeed * _speedBoostModifier : _movementSpeed;
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
            _nextFire = Time.time + _fireRate;
            Vector3 playerPosition = transform.position;
            Vector3 laserSpawnOffset = _tripleShotActive ? playerPosition : playerPosition + _laserOffset;
            GameObject shotPrefab = _tripleShotActive ? _tripleShotPrefab : _laserPrefab;
            Instantiate(shotPrefab, laserSpawnOffset, Quaternion.identity);
        }
    }
    
    public void Damage()
    {
        if (_shieldsActive)
        {
            _shieldStrength--;
            _shieldsActive = _shieldStrength > 0;
            if (_shieldVisualizer != null) { _shieldVisualizer.SetActive(_shieldsActive); }
        }
        else
        {
            _lives--;
            _gameManager.SetLives(_lives);
            if (_lives < 1)
            {
                Destroy(gameObject);
            }
            else
            {
                UpdateEngineDamageVisualizers(_lives);
            }
        }
    }
    
    public void ActivatePowerup(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.TripleShot:
                if (_tripleShotActive) { StopCoroutine(nameof(TripleShotCooldownRoutine)); }
                _tripleShotActive = true;
                StartCoroutine(nameof(TripleShotCooldownRoutine));
                break;
            case PowerupType.SpeedBoost:
                if (_speedBoostActive) { StopCoroutine(nameof(SpeedBoostCooldownRoutine)); }
                _speedBoostActive = true;
                StartCoroutine(nameof(SpeedBoostCooldownRoutine));
                break;
            case PowerupType.Shields:
                _shieldStrength = _maxShieldStrength;
                _shieldsActive = _shieldStrength > 0;
                if (_shieldVisualizer != null) { _shieldVisualizer.SetActive(_shieldsActive); }
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
