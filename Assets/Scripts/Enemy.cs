using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    //Player Reference
    private Player _player;
    
    //Enemy Properties
    [SerializeField] private int _pointValue = 10;
    
    //Movement Properties
    [SerializeField] private float _movementSpeed = 4.0f;
    private float _screenLimitTop = 8.0f;
    private float _screenLimitBottom = -5.0f;
    private float _screenLimitLeft = -8f;
    private float _screenLimitRight = 8f;
    private float _zPos = 0f;
    
    //Animation Properties
    private Animator _animator;
    private Collider2D _collider;

    private void Start()
    {
        _player = GameObject.Find(nameof(Player))?.GetComponent<Player>();
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Animator is NULL in Enemy");
        }
        
        _collider = GetComponent<Collider2D>();
        if (_collider == null)
        {
            Debug.LogError("Collider is NULL in Enemy");
        }
    }

    void Update()
    {
        transform.Translate(Vector3.down * (_movementSpeed * Time.deltaTime));

        if (transform.position.y < _screenLimitBottom)
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
            if (_collider != null) { _collider.enabled = false; }
            if (_animator != null) { _animator.SetTrigger("OnEnemyDeath"); }
            var animationLength = 2.64f; // get dynamically from animator
            Destroy(gameObject, animationLength);
        }

        if (other.CompareTag("Laser"))
        {
            if (_player != null) { _player.IncreaseScore(_pointValue); }
            Destroy(other.gameObject);
            if (_collider != null) { _collider.enabled = false; }
            if (_animator != null) { _animator.SetTrigger("OnEnemyDeath"); }
            var animationLength = 2.64f; // get dynamically from animator
            Destroy(gameObject, animationLength);
        }
    }
}
