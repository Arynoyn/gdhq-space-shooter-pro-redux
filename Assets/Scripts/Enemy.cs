using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _movementSpeed = 4.0f;
    private float _topScreenLimit = 8.0f;
    private float _bottomScreenLimit = -5.0f;
    private float _leftScreenLimit = -8f;
    private float _rightScreenLimit = 8f;
    private float _zPos = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.down * (_movementSpeed * Time.deltaTime));

        if (transform.position.y < _bottomScreenLimit)
        {
            float randomXPos = Random.Range(_leftScreenLimit, _rightScreenLimit);
            transform.position = new Vector3(randomXPos, _topScreenLimit, _zPos);
        }
    }
}