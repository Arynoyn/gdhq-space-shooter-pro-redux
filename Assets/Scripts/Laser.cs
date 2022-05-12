using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private float _speed = 8.0f;
    private float _screenLimitTop = 8.0f;

    private void Update()
    {
        transform.Translate(Vector3.up * (_speed * Time.deltaTime));
        if (transform.position.y > _screenLimitTop)
        {
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }
            Destroy(gameObject);
        }
    }
}