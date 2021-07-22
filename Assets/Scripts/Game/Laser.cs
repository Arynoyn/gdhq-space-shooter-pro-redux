#region

using System;
using UnityEngine;

#endregion

public class Laser : MonoBehaviour
{
    [SerializeField] private protected float _speed = 8.0f;
    [SerializeField] private protected LaserTypeEnum _type = LaserTypeEnum.Player;
    [SerializeField] private protected float _trajectoryAngle = 0.0f;
    [SerializeField] protected float _verticalOffset;
    [SerializeField] protected float _horizontalOffset;
    private bool _hasParent;
    private GameManager _gameManager;
    private protected ViewportBounds _viewportBounds;

    protected virtual void Start()
    {
        SetOffsetPosition();
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

    protected virtual void Update()
    {
        CalculateMovement();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_type == LaserTypeEnum.Enemy && other.CompareTag("Player"))
        {
            DamagePlayer(other);
            DestroyLaser();
        }
    }

    protected static void DamagePlayer(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("Player is NULL during collision with Laser");
        }
        else
        {
            player.Damage();
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

    protected virtual void SetOffsetPosition()
    {
        Vector3 verticalOffsetFix = transform.up * _verticalOffset;
        Vector3 horizontalOffsetFix = transform.right * _horizontalOffset;
        transform.position = transform.position + verticalOffsetFix + horizontalOffsetFix;
    }
}