using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private float _speed = 8.0f;
    private float _screenTop = 8.0f;

    void Update()
    {
        transform.Translate(Vector3.up * (_speed * Time.deltaTime));
        if (transform.position.y > _screenTop)
        {
            Destroy(gameObject);
        }
    }
}
