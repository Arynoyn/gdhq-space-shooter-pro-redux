using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    // Player Reference
    private Player _player;
    
    // Enemy Properties
    [SerializeField] private protected int _pointValue = 100;
    [SerializeField] private float _entrySpeed = 2.0f;
    [SerializeField] private float _entryStoppingPoint = 2.5f;
    [SerializeField] private float _battleSpeed = 4.0f;
    [SerializeField] private int _health = 100;
    [SerializeField] private GameObject _attackPathPrefab;
    private List<Transform> _waypoints;
    private int _currentWaypointIndex;
    private bool _entering;
    private bool _attacking;
    private bool _moving;
    private bool _iteratingForwardsThroughWaypoints = true;
    
    // Audio Properties
    [Header("Audio")]
    [Space]
    [SerializeField] private protected AudioClip _explosionSound;
    private AudioSource _audioSource;
    
    //FireBehavior
    private IFireLaserBehavior _fireLaserBehavior;
    
    // Animation Properties
    private Collider2D _collider;
    [SerializeField] private GameObject _explosionPrefab;
    private int _attackDelay = 1;
    

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.Find(nameof(Player))?.GetComponent<Player>();
        if (_player == null) { LogError("Player is NULL on Enemy!"); }
        
        _collider = GetComponent<Collider2D>();
        if (_collider == null) { LogError("Collider is NULL in Enemy"); }
        
        _fireLaserBehavior = GetComponent<IFireLaserBehavior>();
        if (_fireLaserBehavior == null)
        {
            LogWarning("FireBehavior is NULL in Enemy");
        }
        
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) { LogError("Audio Source is missing on Enemy!"); }
        if (_explosionSound == null) { LogError("Explosion Sound missing from Enemy!"); }
        if (_explosionPrefab == null)
        {
            Debug.LogError("EnemyWithAttachedExplosionAnimation::Start(187): Explosion Prefab is NULL in EnemyWithAttachedExplosionAnimation");
        }
        _waypoints = _attackPathPrefab.transform.Cast<Transform>().ToList();
        _entering = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_entering)
        {
            MoveToCenterOfScreen();
        }
        else if (_attacking)
        {
            MoveTowardsWaypoint();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        //TODO: remove private player instance and notify the game manager instead
        if (other.CompareTag("Player"))
        {
            if (_player != null) { _player.Damage(); }
            GameManager.Instance.ShakeCamera();
            _health -= 10;
        }

        if (other.CompareTag("Laser"))
        {
            Laser laser = other.GetComponent<Laser>();
            if (laser == null || laser.GetOwnerType() != LaserType.Player) { return; }
        
            Destroy(other.gameObject);
            GameManager.Instance.ShakeCamera();
            _health -= 10;
        }
        
        if (_health <= 0)
        {
            ActivateDeath();
        }
    }
    
    private void PlayExplosion()
    {
        if (_explosionPrefab != null)
        {
            GameObject instantiatedExplosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            instantiatedExplosion.transform.localScale = new Vector3(2, 2, 2);
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void ActivateDeath()
    {
        if (_player != null) { _player.IncreaseScore(_pointValue); }
        
        _fireLaserBehavior?.DisableFiring();
        if (_collider != null)
        {
            _collider.enabled = false;
        }

        PlayExplosion();
        if (_audioSource != null)
        {
            _audioSource.PlayOneShot(_explosionSound);
        }

        DestroySelf();
        GameManager.Instance.LoadCredits();
    }

    private void MoveToCenterOfScreen()
    {
        if (transform.position.y >= _entryStoppingPoint) {
            transform.Translate(new Vector3(0, -1, 0) * Time.deltaTime * _entrySpeed);
        }
        else
        {
            _entering = false;
            _attacking = true;
            _moving = true;
        }
    }
    
    private void MoveTowardsWaypoint()
    {
        if (_moving)
        {
            if (_currentWaypointIndex >= _waypoints.Count)
            {
                _iteratingForwardsThroughWaypoints = false;
                _currentWaypointIndex--;
            }
            else if (_currentWaypointIndex < 0)
            {
                _iteratingForwardsThroughWaypoints = true;
                _currentWaypointIndex++;
            }
        
            Vector3 targetWaypointPosition = _waypoints[_currentWaypointIndex].transform.position;
            float moveDistance = _battleSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetWaypointPosition,
                moveDistance);

            if (transform.position == targetWaypointPosition)
            {
                _moving = false;
                StartCoroutine(PauseCoroutine(_attackDelay));
                StartCoroutine(FiringRoutine(_attackDelay));
                if (_iteratingForwardsThroughWaypoints)
                {
                    _currentWaypointIndex++;
                }
                else
                {
                    _currentWaypointIndex--;
                }
            }
        }
    }

    IEnumerator PauseCoroutine(int delay)
    {
        int pauseModifer = (_currentWaypointIndex % 3) + 1;
        yield return new WaitForSeconds(delay * pauseModifer);
        _moving = true;
    }
    
    IEnumerator FiringRoutine(int delay)
    {
        int numberOfLasersToFire = (_currentWaypointIndex % 3) + 1;
        for (int i = 0; i < numberOfLasersToFire; i++)
        {
            yield return new WaitForSeconds(delay);
            _fireLaserBehavior.FireAtClosestTarget();
        }
    }
    
    private void LogError(string message,
        [CallerMemberName] string callingMethod = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        var className = Path.GetFileNameWithoutExtension(callingFilePath);
        Debug.LogError($"{className}::{callingMethod}({callingFileLineNumber}): {message}!");
    }
   
    private void LogWarning(string message,
        [CallerMemberName] string callingMethod = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        var className = Path.GetFileNameWithoutExtension(callingFilePath);
        Debug.LogWarning($"{className}::{callingMethod}({callingFileLineNumber}): {message}!");
    }
}
