using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 10.0f;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameManager _gameManager;


    // Start is called before the first frame update
    void Start()
    {
        if (_explosionPrefab == null)
        {
            Debug.LogError("Explosion Prefab is NULL in Asteroid");
        }
        
        if (_gameManager == null)
        {
            Debug.LogError("Game Manager is NULL in Asteroid");
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * (_rotationSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Laser"))
        {
            Destroy(other.gameObject);
            if (_explosionPrefab != null) { Instantiate(_explosionPrefab, transform.position, Quaternion.identity); }
            _gameManager.StartGame();
            Destroy(gameObject);
        }
    }
}
