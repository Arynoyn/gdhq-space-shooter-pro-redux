using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 3.0f;
    [SerializeField] private PowerupType _type = PowerupType.TripleShot;
    [SerializeField] private float _effectDuration; 
    
    private float _screenLimitBottom = -6.0f;
    void Update()
    {
        transform.Translate(Vector3.down * (_movementSpeed * Time.deltaTime));

        if (transform.position.y < _screenLimitBottom)
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
    
    public PowerupType GetPowerupType()
    {
        return _type;
    }
    
    public float GetEffectDuration() 
    { 
        return _effectDuration; 
    }
}
