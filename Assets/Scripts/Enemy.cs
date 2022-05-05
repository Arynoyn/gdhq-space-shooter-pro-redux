using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 4.0f;
    private float _screenLimitTop = 8.0f;
    private float _screenLimitBottom = -5.0f;
    private float _screenLimitLeft = -8f;
    private float _screenLimitRight = 8f;
    private float _zPos = 0f;

    private void Update()
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
            Player player = other.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.Damage();
            }

            Destroy(gameObject);
        }

        if (other.CompareTag("Laser"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}