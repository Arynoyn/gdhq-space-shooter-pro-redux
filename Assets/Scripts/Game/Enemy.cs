using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Enemy : MonoBehaviour
{
    // Player Reference
    private Player _player;
    
    // Managers
    private GameManager _gameManager;
    
    // Enemy Properties
    [SerializeField] private protected int _pointValue = 10;
    private bool _isDestroyed;
    
    // Movement Properties
    [Header("Movement")]
    [Space]
    [SerializeField] private float _movementSpeed = 4.0f;

    private ViewportBounds _viewportBounds;
    /*private float _screenLimitTop = 8.0f;
    private float _screenLimitBottom = -5.0f;
    private float _screenLimitLeft = -8f;
    private float _screenLimitRight = 8f;*/
    private float _zPos = 0f;
    
    // Attack Properties
    [Header("Attacks")]
    [Space]
    [SerializeField] private AudioClip _laserSound;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private float _fireRate = 0.50f;
    private readonly Vector3 _laserOffset = new Vector3(0, -1.05f, 0);
    private float _nextFireDelay;
    private bool _fireActive = true;
    private IEnumerator _fireLaserRoutine;
    
    //Animation
    private protected delegate void PlayExplosionDelegate();
    private protected PlayExplosionDelegate PlayExplosion = PlayExplosionMethod;
    private protected delegate void DestroySelfDelegate();
    private protected DestroySelfDelegate DestroySelf = DestroySelfMethod;
    private Collider2D _collider;
    
    // Audio Properties
    [Header("Audio")]
    [Space]
    [SerializeField] private protected AudioClip _explosionSound;
    private AudioSource _audioSource;
    
    protected virtual void Start()
    {
        _player = GameObject.Find(nameof(Player))?.GetComponent<Player>();
        if (_player == null) { Debug.LogError("Enemy::Start(52): Player is NULL on Enemy!"); }
        
        _gameManager = FindObjectOfType<GameManager>();
        if (_gameManager == null) { Debug.LogError("Enemy::Start(55): Game Manager is NULL on Enemy!"); }
        else
        {
            _viewportBounds = _gameManager.GetViewportBounds();
            if (_viewportBounds == null)
            {
                Debug.LogError("Enemy::Start(61): Viewport Bounds is NULL on Enemy!");
            }
        }
        
        if (_laserPrefab == null) { Debug.LogError("Enemy::Start(65): LaserPrefab is NULL on Enemy!"); }
        
        _collider = GetComponent<Collider2D>();
        if (_collider == null) { Debug.LogError("Enemy::Start(68): Collider is NULL in Enemy"); }
        
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) { Debug.LogError("Enemy::Start(71): Audio Source is missing on Enemy!"); }
        if (_explosionSound == null) { Debug.LogError("Enemy::Start(72): Explosion Sound missing from Enemy!"); }
        if (_laserSound == null) { Debug.LogError("Enemy::Start(73): Laser Sound missing from Enemy!"); }
        
        _nextFireDelay = Random.Range(_fireRate, _fireRate * 2);
        
        _fireLaserRoutine = FireLaserRoutine();
        StartCoroutine(_fireLaserRoutine);
    }

    void Update()
    {
        transform.Translate(Vector3.down * (_movementSpeed * Time.deltaTime));

        if (transform.position.y < _viewportBounds.Bottom && !_isDestroyed)
        {
            float randomXPos = Random.Range(_viewportBounds.Left, _viewportBounds.Right);
            transform.position = new Vector3(randomXPos, _viewportBounds.Top, _zPos);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_player != null) { _player.Damage(); }
            _isDestroyed = true;
            _fireActive = false;
            if (_collider != null) { _collider.enabled = false; }
            PlayExplosion();
            if (_audioSource != null) { _audioSource.PlayOneShot(_explosionSound); }
            DestroySelf();
        }

        if (other.CompareTag("Laser"))
        {
            Laser laser = other.GetComponent<Laser>();
            if (laser == null || laser.GetOwnerType() != LaserTypeEnum.Player)
            {
                return;
            }

            if (_player != null) { _player.IncreaseScore(_pointValue); }
            Destroy(other.gameObject);
            _isDestroyed = true;
            if (_collider != null) { _collider.enabled = false; }
            PlayExplosion();
            if (_audioSource != null) { _audioSource.PlayOneShot(_explosionSound); }
            DestroySelf();
        }
    }
    
    IEnumerator FireLaserRoutine()
    {
        while (_fireActive)
        {
            yield return new WaitForSeconds(_nextFireDelay);
            FireLaser();
        }
    }
    
    private void FireLaser()
    {
        Instantiate(_laserPrefab, transform.position + _laserOffset, Quaternion.identity);
        _audioSource.PlayOneShot(_laserSound);
        CalculateNextFireTime();
    }

    private void CalculateNextFireTime()
    {
        _nextFireDelay = Random.Range(3.0f, 7.0f);
    }
    
    private static void DestroySelfMethod()
    {
        Debug.LogError("Enemy::DestroySelfMethod(87): Destroy Self Delegate not defined!");
    }

    private static void PlayExplosionMethod()
    {
        Debug.LogError("Enemy::PlayExplosionMethod(92): Play Explosion Delegate not defined!");
    }
}