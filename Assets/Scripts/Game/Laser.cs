using System;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private protected float _speed = 8.0f;
    [SerializeField] private protected LaserType _type = LaserType.Player;
    [SerializeField] private protected float _trajectoryAngle = 0.0f;
    [SerializeField] protected float _verticalOffset;
    [SerializeField] protected float _horizontalOffset;
    private bool _hasParent;
    
    protected ViewportBounds _viewportBounds;

    protected virtual void Start()
    {
        SetOffsetPosition();
        
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

    protected virtual void Update()
    {
        CalculateMovement();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_type == LaserType.Enemy && other.CompareTag("Player"))
        {
            DamagePlayer(other);
            DestroyLaser();
        }
    }
    
    protected static void DamagePlayer(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) { Debug.LogError("Player is NULL during collision with Laser"); }
        else { player.Damage(); }
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
    
    protected virtual void SetOffsetPosition()
    {
        Vector3 verticalOffsetFix = transform.up * _verticalOffset;
        Vector3 horizontalOffsetFix = transform.right * _horizontalOffset;
        transform.position = transform.position + verticalOffsetFix + horizontalOffsetFix;
    }
}