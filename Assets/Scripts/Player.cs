using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, Controls.IPlayerActions
{
    private Vector2 _direction;

    [SerializeField] private float _movementSpeed = 3.5f;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private int _lives = 3;
    
    private float _topMovementLimit = 0f;
    private float _bottomMovementLimit = -3.8f;
    private float _leftMovementLimit = -9.5f;
    private float _rightMovementLimit = 9.5f;
    private float _verticalStartPosition = -2.0f;
    private float _horizontalStartPosition = 0f;
    private float _zPos = 0f;
    
    private Vector3 _laserOffset = new Vector3(0f, 0.8f, 0f);
    private float _nextFire = -1f;
    private SpawnManager _spawnManager;

    void Start()
    {
        transform.position = new Vector3(_horizontalStartPosition, _verticalStartPosition, _zPos);
        _spawnManager = GameObject.FindObjectOfType<SpawnManager>();
        if (_spawnManager == null)
        {
            Debug.LogError("Spawn Manager in Player class is NULL");
        }
    }

    void Update()
    {       
        CalculateMovement();
    }

    private void CalculateMovement()
    {
        transform.Translate(_direction * (_movementSpeed * Time.deltaTime));
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
            Vector3 laserSpawnOffset = transform.position + _laserOffset;
            Instantiate(_laserPrefab, laserSpawnOffset, Quaternion.identity);
        }
    }
    
    public void Damage()
    {
        _lives--;
        if (_lives < 1)
        {
            _spawnManager.StopSpawningEnemies();
            Destroy(gameObject);
        }
    }
}
