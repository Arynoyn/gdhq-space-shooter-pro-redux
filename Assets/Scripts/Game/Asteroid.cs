using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 10.0f;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameManager _gameManager;


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
            _gameManager.StartGame();
            Destroy(gameObject);
        }
    }
}
