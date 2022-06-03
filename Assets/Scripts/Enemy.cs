using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    // Player Reference
    private Player _player;

    // Enemy Properties
    [SerializeField] private int _pointValue = 10;
    private bool _isDestroyed;
    
    // Movement Properties
    [Header("Movement")]
    [Space]
    [SerializeField] private float _movementSpeed = 4.0f;
    private float _screenLimitTop = 8.0f;
    private float _screenLimitBottom = -5.0f;
    private float _screenLimitLeft = -8f;
    private float _screenLimitRight = 8f;
    private float _zPos = 0f;
    
    // Animation Properties
    [Header("Animation")]
    [Space]
    [SerializeField] private string _enemyDestroyedAnimationName = "Enemy_Destroyed_anim";
    [SerializeField] private string _enemyDeathTriggerName = "OnEnemyDeath";
    private Animator _animator;
    private Collider2D _collider;
    private float _deathAnimationLength;
    
    // Audio Properties
    [Header("Audio")]
    [Space]
    [SerializeField] private AudioClip _explosionSound;
    private AudioSource _audioSource;

    private void Start()
    {
        _player = GameObject.Find(nameof(Player))?.GetComponent<Player>();
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Animator is NULL on Enemy");
        }
        else
        {
            _deathAnimationLength = GetAnimationLength(_enemyDestroyedAnimationName);
        }
        
        _collider = GetComponent<Collider2D>();
        if (_collider == null)
        {
            Debug.LogError("Collider is NULL in Enemy");
        }
        
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) { Debug.LogError("Audio Source is missing on Enemy!"); }
        if (_explosionSound == null) { Debug.LogError("Explosion Sound missing from Enemy!"); }
    }
    
    private void Update()
    {
        transform.Translate(Vector3.down * (_movementSpeed * Time.deltaTime));

        if (transform.position.y < _screenLimitBottom && !_isDestroyed)
        {
            float randomXPos = Random.Range(_screenLimitLeft, _screenLimitRight);
            transform.position = new Vector3(randomXPos, _screenLimitTop, _zPos);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_player != null) { _player.Damage(); }
            _isDestroyed = true;
            if (_collider != null) { _collider.enabled = false; }
            if (_animator != null) { _animator.SetTrigger(_enemyDeathTriggerName); }
            if (_audioSource != null) { _audioSource.PlayOneShot(_explosionSound); }
            Destroy(gameObject, _deathAnimationLength);
        }

        if (other.CompareTag("Laser"))
        {
            if (_player != null) { _player.IncreaseScore(_pointValue); }
            Destroy(other.gameObject);
            _isDestroyed = true;
            if (_collider != null) { _collider.enabled = false; }
            if (_animator != null) { _animator.SetTrigger(_enemyDeathTriggerName); }
            if (_audioSource != null) { _audioSource.PlayOneShot(_explosionSound); }
            Destroy(gameObject, _deathAnimationLength);
        }
    }
    
    private float GetAnimationLength(string animationName)
    {
        AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
        var deathAnimationClip = clips.FirstOrDefault(clip => clip.name == animationName);
        if (deathAnimationClip == null)
        {
            Debug.LogError("Death Animation clip is NULL in animator on Enemy");
        }
        else
        {
            return deathAnimationClip.length;
        }

        return 0f;
    }
}