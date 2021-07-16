using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Wave")]
public class Wave : ScriptableObject
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _pathPrefab;
    [SerializeField] private float _timeBetweenSpawns = 0.3f;
    [SerializeField] private int _numberOfEnemies = 5;
    [SerializeField] private float _movementSpeed = 5f;
    
    public GameObject GetEnemyPrefab() => _enemyPrefab;
    public List<Transform> GetWaypoints() => _pathPrefab.transform.Cast<Transform>().ToList();
    public float GetTimeBetweenSpawns() => _timeBetweenSpawns;
    public int GetNumberOfEnemies() => _numberOfEnemies;
    public float GetMovementSpeed() => _movementSpeed;

    public bool HasPath => _pathPrefab is { } && _enemyPrefab.TryGetComponent<EnemyPathing>(out _);

    private void OnValidate()
    {
        if (_timeBetweenSpawns < 0)
        {
            _timeBetweenSpawns = 0;
        }
        
        if (_numberOfEnemies < 0)
        {
            _numberOfEnemies = 0;
        }
        
        if (_movementSpeed < 0)
        {
            _movementSpeed = 0;
        }
    }
}
