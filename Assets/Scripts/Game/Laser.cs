#region

using System;
using UnityEngine;

#endregion

public class Laser : MonoBehaviour
{
    [SerializeField] private float _speed = 8.0f;
    [SerializeField] private LaserTypeEnum _type = LaserTypeEnum.Player;
    [SerializeField] private float _trajectoryAngle = 0.0f;
    private bool _hasParent;
    private GameManager _gameManager;
    private ViewportBounds _viewportBounds;
    // private float _screenLimitTop = 8.0f;
    // private float _screenLimitBottom = -5.0f;


    private void Start()
    {
        _hasParent = transform.parent != null;
        _gameManager = FindObjectOfType<GameManager>();
        if (_gameManager == null) { Debug.LogError("Game Manager is NULL on Laser!"); }
        else
        {
            _viewportBounds = _gameManager.GetViewportBounds();
            if (_viewportBounds == null)
            {
                Debug.LogError("Viewport Bounds is NULL on Laser!");
            }
        }
    }

    private void Update()
    {
        CalculateMovement();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_type == LaserTypeEnum.Enemy && other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player == null) { Debug.LogError("Player is NULL during collision with Laser"); }
            else { player.Damage(); }
            DestroyLaser();
        }
    }

    private void CalculateMovement()
    {
        Vector3 movementVector = (Quaternion.Euler(0, 0 , _trajectoryAngle) * Vector3.up).normalized;
        transform.Translate(movementVector * (_speed * Time.deltaTime));
        
        if (transform.position.y > _viewportBounds.Top || transform.position.y < _viewportBounds.Bottom)
        {
            DestroyLaser();
        }
    }

    private void DestroyLaser()
    {
        if (_hasParent)
        {
            Destroy(transform.parent.gameObject);
        }

        Destroy(gameObject);
    }

    public LaserTypeEnum GetOwnerType()
    {
        return _type;
    }

    public void SetType(LaserTypeEnum type)
    {
        _type = type;
    }
    
    public float GetAngle()
    {
        return _trajectoryAngle;
    }
    public void SetAngle(float angle)
    {
        _trajectoryAngle = angle;
        transform.rotation = Quaternion.Euler(0,0, _trajectoryAngle);
    }
}