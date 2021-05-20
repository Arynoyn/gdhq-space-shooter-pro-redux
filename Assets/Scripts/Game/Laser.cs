#region

using System;
using UnityEngine;

#endregion

public class Laser : MonoBehaviour
{
    [SerializeField] private float _speed = 8.0f;
    [SerializeField] private LaserTypeEnum _type = LaserTypeEnum.Player;
    private bool _hasParent;
    private float _screenLimitTop = 8.0f;
    private float _screenLimitBottom = -5.0f;


    private void Start()
    {
        _hasParent = transform.parent != null;
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
        Vector3 movementDirection = _type switch
        {
            LaserTypeEnum.Player => Vector3.up,
            LaserTypeEnum.Enemy => Vector3.down,
            _ => throw new ArgumentOutOfRangeException(nameof(_type), $"Not expected Laser Type value: {_type}")
        };
        
        transform.Translate(movementDirection * (_speed * Time.deltaTime));
        if (transform.position.y > _screenLimitTop || transform.position.y < _screenLimitBottom)
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
}