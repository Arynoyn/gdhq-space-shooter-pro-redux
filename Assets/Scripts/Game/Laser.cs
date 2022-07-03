using System;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private float _speed = 8.0f;
    [SerializeField] private LaserType _type = LaserType.Player;
    [SerializeField] private float _trajectoryAngle = 0.0f;
    private bool _hasParent;
    private ViewportBounds _viewportBounds;

    private void Start()
    {
        _hasParent = transform.parent != null;
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("Game Manager is NULL");
        }
        else
        {
            _viewportBounds = GameManager.Instance.GetViewportBounds();
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
        if (_type == LaserType.Enemy && other.CompareTag("Player"))
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
        //TODO: Don't destroy parent if there are still active lasers on screen (SprayShot)
        if (_hasParent)
        {
            Destroy(transform.parent.gameObject);
        }

        Destroy(gameObject);
    }
    
    public LaserType GetOwnerType()
    {
        return _type;
    }

    public void SetType(LaserType type)
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