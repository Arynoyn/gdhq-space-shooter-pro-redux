using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, Controls.IPlayerActions
{
    private Vector2 _direction;

    [SerializeField] private float _speed = 3.5f;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector2(0f,-3.9f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(_direction * (_speed * Time.deltaTime));
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _direction = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }
}
