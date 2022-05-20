using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, Controls.IPlayerActions
{
    private Vector2 _direction;

    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private GameObject _tripleShotPrefab;
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _speedBoostModifier = 2.0f;
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private int _lives = 3;
    [SerializeField] private Animator _animator;
    [SerializeField] private int _maxShieldStrength = 3;
    
    private float _topMovementLimit = 0f;
    private float _bottomMovementLimit = -3.8f;
    private float _leftMovementLimit = -9.5f;
    private float _rightMovementLimit = 9.5f;
    private float _verticalStartPosition = -2.0f;
    private float _horizontalStartPosition = 0f;
    private float _zPos = 0f;
    
    private SpawnManager _spawnManager;
    private Vector3 _laserOffset = new Vector3(0f, 1.25f, 0f);
    private float _nextFire = -1f;
    
    private bool _tripleShotActive;
    private bool _speedBoostActive;
    private bool _shieldsActive;
    private int _shieldStrength;
    private GameObject _shieldVisualizer;
    
    private bool _isMovingRight;
    private bool _isMovingLeft;

    void Start()
    {
        transform.position = new Vector3(_horizontalStartPosition, _verticalStartPosition, _zPos);
        
        _spawnManager = FindObjectOfType<SpawnManager>();
        if (_spawnManager == null)
        {
            Debug.LogError("Spawn Manager in Player class is NULL");
        }
        
        _shieldVisualizer = transform.Find("Shield_Visualizer").gameObject;
        if (_shieldVisualizer == null)
        {
            Debug.LogError("Shield Visualizer in Player class is NULL");
        }
        else
        {
            _shieldVisualizer.SetActive(false);
        }
        
        if (_laserPrefab == null)
        {
            Debug.LogError("Laser Prefab in Player class is NULL");
        }
        
        if (_tripleShotPrefab == null)
        {
            Debug.LogError("Triple Shot Prefab in Player class is NULL");
        }
    }

    void Update()
    {       
        CalculateMovement();
    }

    private void CalculateMovement()
    {
        float modifiedSpeed = _speedBoostActive ? _movementSpeed * _speedBoostModifier : _movementSpeed;
        _animator.SetBool("isTurningLeft", _direction.x < 0);
        _animator.SetBool("isTurningRight", _direction.x > 0);
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
            if (_lives < 1)
            {
                _spawnManager.StopSpawningEnemies();
                _spawnManager.StopSpawningPowerups();
                Destroy(gameObject);
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
