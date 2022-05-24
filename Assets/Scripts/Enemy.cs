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

    private void Start()
    {
        _player = GameObject.Find(nameof(Player))?.GetComponent<Player>();
    }
    
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
            if (_player != null) { _player.Damage(); }

            Destroy(gameObject);
        }

        if (other.CompareTag("Laser"))
        {
            if (_player != null) { _player.IncreaseScore(_pointValue); }
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}