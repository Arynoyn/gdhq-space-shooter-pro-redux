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
    private float _leftMovementLimit = -9.5f;
    private float _rightMovementLimit = 9.5f;
    private float _verticalStartPosition = -2.0f;
    private float _horizontalStartPosition = 0f;
    private float _zPos = 0;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(_horizontalStartPosition, _verticalStartPosition, _zPos);
    }

    // Update is called once per frame
    void Update()
    {       
        CalculateMovement();
    }

    private void CalculateMovement()
    {
        transform.Translate(_direction * (_speed * Time.deltaTime));
        
        bool shouldTeleport = transform.position.x <= _leftMovementLimit || transform.position.x >= _rightMovementLimit;
        float yPosClamped = Mathf.Clamp(transform.position.y, _bottomMovementLimit, _topMovementLimit);
        float xPos = shouldTeleport ? -transform.position.x : transform.position.x;
        transform.position = new Vector3(xPos, yPosClamped, _zPos);
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
