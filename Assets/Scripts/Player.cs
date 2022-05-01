using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, Controls.IPlayerActions
{
    private Vector2 _direction;

    [SerializeField] private float _speed = 3.5f;
    private float _topMovementLimit = 0f;
    private float _bottomMovementLimit = -3.8f;
    private float _leftMovementLimit = -8.5f;
    private float _rightMovementLimit = 8.5f;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector2(0f,-3.9f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(_direction * (_speed * Time.deltaTime));
        
        float yClamped = Mathf.Clamp(transform.position.y, _bottomMovementLimit, _topMovementLimit);
        transform.position = new Vector3(transform.position.x, yClamped, 0);

        
        if (transform.position.x <= _leftMovementLimit || transform.position.x >= _rightMovementLimit)
        {
            transform.position = new Vector3(-transform.position.x, transform.position.y, 0);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _direction = context.ReadValue<Vector2>().normalized;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }
}
