using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour, Controls.IPlayerActions
{
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private GameObject _tripleShotPrefab;
    [SerializeField] private float _movementSpeed = 3.5f;
    [SerializeField] private float _speedBoostModifier = 2.0f;
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private int _lives = 3;
    [SerializeField] private Animator _animator;
    
    private Vector3 _direction;
    private float _verticalStartPosition = -2.0f;
    private float _horizontalStartPosition = 0f;
    private float _topMovementLimit = 0f;
    private float _bottomMovementLimit = -3.8f;
    private float _leftMovementLimit = -11.4f;
    private float _rightMovementLimit = 11.4f;
    private float _zPos = 0f;

    private Vector3 _laserOffset = new Vector3(0f, 1.0f, 0f);
    private float _nextFire = -1f;
    [SerializeField] private bool _tripleShotActive;
    [SerializeField] private bool _speedBoostActive;
    private SpawnManager _spawnManager;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(_horizontalStartPosition, _verticalStartPosition, _zPos);
        _spawnManager = GameObject.FindObjectOfType<SpawnManager>();
        if (_spawnManager == null)
        {
            Debug.LogError("Spawn Manager in Player class is NULL");
        }

        if (_animator == null)
        {
            Debug.LogError("Player Animator in Player class is NULL");
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

    // Update is called once per frame
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
        _lives--;
        if (_lives < 1)
        {
            _spawnManager.StopSpawningEnemies();
            _spawnManager.StopSpawningPowerups();
            Destroy(gameObject);
        }
    }

    public void ActivatePowerup(PowerupTypeEnum type)
    {
        switch (type)
        {
            case PowerupTypeEnum.TripleShot:
                if (_tripleShotActive) { StopCoroutine(nameof(TripleShotCooldownRoutine)); }
                _tripleShotActive = true;
                StartCoroutine(nameof(TripleShotCooldownRoutine));
                break;
            case PowerupTypeEnum.SpeedBoost:
                if (_speedBoostActive) { StopCoroutine(nameof(SpeedBoostCooldownRoutine)); }
                _speedBoostActive = true;
                StartCoroutine(nameof(SpeedBoostCooldownRoutine));
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