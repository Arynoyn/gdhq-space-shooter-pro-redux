using System;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 3.0f;
    [SerializeField] private PowerupTypeEnum _type = PowerupTypeEnum.TripleShot;
    [SerializeField] private float _effectDuration = 5.0f;
    private GameManager _gameManager;
    private ViewportBounds _viewportBounds;
    // private float _screenLimitBottom = -6.0f;

    private void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        if (_gameManager == null) { Debug.LogError("Game Manager is NULL on Powerup!"); }
        else
        {
            _viewportBounds = _gameManager.GetViewportBounds();
            if (_viewportBounds == null)
            {
                Debug.LogError("Viewport Bounds is NULL on Powerup!");
            }
        }
    }

    void Update()
    {
        transform.Translate(Vector3.down * (_movementSpeed * Time.deltaTime));

        if (transform.position.y < _viewportBounds.Bottom)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.ActivatePowerup(this);
            }
            Destroy(gameObject);
        }
    }
    public PowerupTypeEnum GetPowerupType()
    {
        return _type;
    }
    
    public float GetEffectDuration() 
    { 
        return _effectDuration; 
    }
}
