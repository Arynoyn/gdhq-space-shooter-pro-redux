using System;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 3.0f;
    [SerializeField] private PowerupType _type = PowerupType.TripleShot;
    [SerializeField] private float _effectDuration; 
    [SerializeField] private int _spawnWeight;
    private Vector3 _movementDirection = Vector3.down;
    
    private ViewportBounds _viewportBounds;
    private Vector3 _defaultMovementDirection;
    private float _defaultMovementSpeed;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("Game Manager is NULL");
        }
        else
        {
            _viewportBounds = GameManager.Instance.GetViewportBounds();
            if (_viewportBounds == null)
            {
                Debug.LogError("Viewport Bounds is NULL on Powerup!");
            }
        }

        _defaultMovementDirection = _movementDirection;
        _defaultMovementSpeed = _movementSpeed;
    }

    private void Update()
    {
        transform.Translate(_movementDirection * (_movementSpeed * Time.deltaTime));

        if (transform.position.y < _viewportBounds.Bottom)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Player player = other.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.ActivatePowerup(this);
            }
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Laser"))
        {
            Laser laser = other.GetComponent<Laser>();
            if (laser == null || laser.GetOwnerType() != LaserType.Enemy)
            {
                return;
            }
            Destroy(gameObject);
        }
    }
    
    public PowerupType GetPowerupType()
    {
        return _type;
    }
    
    public float GetEffectDuration() 
    { 
        return _effectDuration; 
    }
    
    public int GetSpawnWeight()
    {
        return _spawnWeight;
    }

    public void MoveTowardsPosition(Vector3 targetPosition)
    {
        _movementDirection = (targetPosition - transform.position).normalized;
        _movementSpeed += 0.5f;
    }

    public void ResumeDefaultMovement()
    {
        _movementDirection = _defaultMovementDirection;
        _movementSpeed = _defaultMovementSpeed;
    }
}
