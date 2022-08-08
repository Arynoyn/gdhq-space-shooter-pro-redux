using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Enemy : MonoBehaviour
{
    // Player Reference
    private Player _player;

    // Enemy Properties
    [SerializeField] private protected int _pointValue = 10;
    private bool _isDestroyed;
    
    // Movement Properties
    [Header("Movement")]
    [Space]
    [SerializeField] private float _movementSpeed = 4.0f;
    private Vector3 _movementDirection = Vector3.down;

    private ViewportBounds _viewportBounds;
    private float _zPos = 0f;
    
    // Shield Properties
    [Header("Shields")]
    [SerializeField] private int _maxShieldStrength = 1;
    [SerializeField,Range(0, 100)] private int _shieldChancePercentage = 20;
    private bool _shieldsActive;
    private int _shieldStrength;
    
    //FireBehavior
    private IFireLaserBehavior _fireLaserBehavior;
    
    // Animation Properties
    private protected delegate void PlayExplosionDelegate();
    private protected PlayExplosionDelegate PlayExplosion = PlayExplosionMethod;
    private protected delegate void DestroySelfDelegate();
    private protected DestroySelfDelegate DestroySelf = DestroySelfMethod;
    private Collider2D _collider;
    
    //Visualizers
    private GameObject _shieldVisualizer;
    
    // Audio Properties
    [Header("Audio")]
    [Space]
    [SerializeField] private protected AudioClip _explosionSound;
    private AudioSource _audioSource;
    
    protected virtual void Start()
    {
        if (GameManager.Instance == null)
        {
            LogError("Game Manager is NULL");
        }
        else
        {
            _viewportBounds = GameManager.Instance.GetViewportBounds();
            if (_viewportBounds == null)
            {
                LogError("Viewport Bounds is NULL on Enemy!");
            }
        }
        
        _player = GameObject.Find(nameof(Player))?.GetComponent<Player>();
        if (_player == null) { LogError("Player is NULL on Enemy!"); }

        _collider = GetComponent<Collider2D>();
        if (_collider == null) { LogError("Collider is NULL in Enemy"); }
        
        _shieldVisualizer = transform.Find("Shield_Visualizer")?.gameObject;
        if (_shieldVisualizer == null)
        {
            LogError("Shield Visualizer in Enemy class is NULL");
        }
        else
        {
            _shieldStrength = Random.Range(0, 100) < _shieldChancePercentage ? _maxShieldStrength : 0; //20% chance
            _shieldsActive = _shieldStrength > 0;
            if (_shieldVisualizer != null) { _shieldVisualizer.SetActive(_shieldsActive); }
        }

        _fireLaserBehavior = GetComponent<IFireLaserBehavior>();
        if (_fireLaserBehavior == null)
        {
            LogWarning("FireBehavior is NULL in Enemy");
        }
        
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) { LogError("Audio Source is missing on Enemy!"); }
        if (_explosionSound == null) { LogError("Explosion Sound missing from Enemy!"); }
        
    }
    
    private void Update()
    {
        CalculateMovement();
        Transform powerupTransform = DetectPowerup();
        if (powerupTransform is { })
        {
            _fireLaserBehavior?.Fire(powerupTransform);
        }
    }

    private Transform DetectPowerup()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position,Vector2.down);
        return hit
            ? hit.collider.CompareTag("Powerup")
                ? hit.transform
                : null
            : null;
    }

    private void CalculateMovement()
    {
        transform.Translate(_movementDirection * (_movementSpeed * Time.deltaTime)); 
        if (transform.position.y < _viewportBounds.Bottom && !_isDestroyed)
        {
            RelocateToTopOfScreenWithRandomHorizontalPosition();
        }
    }

    private void RelocateToTopOfScreenWithRandomHorizontalPosition()
    {
        float randomXPos = Random.Range(_viewportBounds.Left, _viewportBounds.Right);
        transform.position = new Vector3(randomXPos, _viewportBounds.Top, _zPos);
    }

    public float GetMovementSpeed()
    {
        return _movementSpeed;
    }
    
    public void SetMovementSpeed(float newMovementSpeed)
    {
        _movementSpeed = newMovementSpeed;
    }

    public void SetTrajectoryAngle(float angle)
    {
        transform.rotation = Quaternion.Euler(new Vector3(0,0,angle));
    }
    
     
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_shieldsActive)
        {
            if (!other.CompareTag("Player") && !other.CompareTag("Laser"))
            {
                return;
            }
            
            if (other.CompareTag("Laser"))
            {
                Laser laser = other.GetComponent<Laser>();
                if (laser == null || laser.GetOwnerType() != LaserType.Player)
                {
                    return;
                }
                Destroy(other.gameObject);
            }

            if (other.CompareTag("Player"))
            {
                if (_player != null) { _player.Damage(); }
            }

            _shieldStrength--;
            _shieldsActive = _shieldStrength > 0;
            if (_shieldVisualizer != null) { _shieldVisualizer.SetActive(_shieldsActive); }
            if (_audioSource != null) { _audioSource.PlayOneShot(_explosionSound); }
        }
        else
        {
            //TODO: remove private player instance and notify the game manager instead
            if (other.CompareTag("Player"))
            {
                if (_player != null) { _player.Damage(); }
                _isDestroyed = true;
                _fireLaserBehavior?.DisableFiring();
                if (_collider != null) { _collider.enabled = false; }
                PlayExplosion();
                if (_audioSource != null) { _audioSource.PlayOneShot(_explosionSound); }
                DestroySelf();
            }

            if (other.CompareTag("Laser"))
            {
                Laser laser = other.GetComponent<Laser>();
                if (laser == null || laser.GetOwnerType() != LaserType.Player) { return; }
            
                if (_player != null) { _player.IncreaseScore(_pointValue); }
                Destroy(other.gameObject);
                _isDestroyed = true;
                _fireLaserBehavior?.DisableFiring();
                if (_collider != null) { _collider.enabled = false; }
                PlayExplosion();
                if (_audioSource != null) { _audioSource.PlayOneShot(_explosionSound); }
                DestroySelf();
            }
        }
        
    }
    
    private static void DestroySelfMethod()
    {
        LogError("Destroy Self Delegate not defined!");
    }

    private static void PlayExplosionMethod()
    {
        LogError("Play Explosion Delegate not defined!");
    }
    
   private static void LogError(string message,
        [CallerMemberName] string callingMethod = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        var className = Path.GetFileNameWithoutExtension(callingFilePath);
        Debug.LogError($"{className}::{callingMethod}({callingFileLineNumber}): {message}!");
    }
   
   private static void LogWarning(string message,
       [CallerMemberName] string callingMethod = "",
       [CallerFilePath] string callingFilePath = "",
       [CallerLineNumber] int callingFileLineNumber = 0)
   {
       var className = Path.GetFileNameWithoutExtension(callingFilePath);
       Debug.LogWarning($"{className}::{callingMethod}({callingFileLineNumber}): {message}!");
   }
}