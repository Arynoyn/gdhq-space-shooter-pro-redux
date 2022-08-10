using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 10.0f;
    [SerializeField] private GameObject _explosionPrefab;
    

    void Start()
    {
        if (_explosionPrefab == null)
        {
            Debug.LogError("Explosion Prefab is NULL in Asteroid");
        }
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("Game Manager is NULL in Asteroid");
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * (_rotationSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Laser")) { return; }

        Laser laser = other.GetComponent<Laser>();
        if (laser == null)
        {
            Debug.LogError("Laser component missing on collided object in Asteroid");
        }
        else
        {
            LaserType laserType = laser.GetOwnerType();
            if (laserType != LaserType.Player) { return; }

            Destroy(other.gameObject);
            if (_explosionPrefab != null) { Instantiate(_explosionPrefab, transform.position, Quaternion.identity); }
            GameManager.Instance.StartGame();
            Destroy(gameObject);
        }
    }
}
