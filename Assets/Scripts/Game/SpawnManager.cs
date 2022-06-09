#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private float _enemySpawnRate = 5.0f;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _enemyContainer;
    [SerializeField] private int _powerupMinSpawnRate = 3;
    [SerializeField] private int _powerupMaxSpawnRate = 7;
    
    private Dictionary<PowerupType, GameObject> _powerupPrefabs;
    
    private float _screenLimitLeft = -8f;
    private float _screenLimitRight = 8f;
    private float _screenLimitTop = 8.0f;
    private float _zPos = 0f;
    
    private bool _spawnEnemies;
    private bool _spawnPowerups;

    [SerializeField] private float _spawnStartDelayTime = 2.0f;
    private WaitForSeconds _spawnStartDelay;

    private void Start()
    {
        _powerupPrefabs = new Dictionary<PowerupType, GameObject>();
        InitializePowerupPrefabsDictionary();
        if (_powerupPrefabs == null)
        {
            Debug.LogError("Powerups array on SpawnManager is NULL");
        }
        else if (!_powerupPrefabs.Any())
        {
            Debug.LogWarning("No powerups in array on SpawnManager");
        }
        
        _spawnStartDelay = new WaitForSeconds(_spawnStartDelayTime);
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (_spawnEnemies)
        {
            float xPos = Random.Range(_screenLimitLeft, _screenLimitRight);
            Vector3 spawnPosition = new Vector3(xPos, _screenLimitTop, _zPos);
            Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity, _enemyContainer.transform);
            yield return new WaitForSeconds(_enemySpawnRate);
        }
    }

    private IEnumerator SpawnPowerupRoutine()
    {
        yield return _spawnStartDelay;
        
        while (_spawnPowerups)
        {
            int randomSpawnTime = Random.Range(_powerupMinSpawnRate, _powerupMaxSpawnRate + 1);
            yield return new WaitForSeconds(randomSpawnTime);
            if (_powerupPrefabs.Any())
            {
                var xPos = Random.Range(_screenLimitLeft, _screenLimitRight);
                var spawnPosition = new Vector3(xPos, _screenLimitTop, _zPos);
                
                GameObject randomPowerup = GetRandomPowerupPrefab();
                Instantiate(randomPowerup, spawnPosition, Quaternion.identity);
            }
        }
    }
    
    private GameObject GetRandomPowerupPrefab()
    {
        var powerupEnumValuesList = Enum.GetValues(typeof(PowerupType)).Cast<PowerupType>().ToList();

        var minValue = powerupEnumValuesList.Min();
        var maxValue = powerupEnumValuesList.Max();
        var randomPowerupType = (PowerupType) Random.Range((int) minValue, (int) maxValue + 1);

        if (!_powerupPrefabs.TryGetValue(randomPowerupType, out var randomPowerup))
        {
            throw new ArgumentOutOfRangeException(nameof(randomPowerupType), randomPowerupType,
                $"{randomPowerupType.ToString()} does not have a matching prefab in the dictionary");
        }

        return randomPowerup;
    }

    public void StartSpawningEnemies()
    {
        _spawnEnemies = true;
        StartCoroutine(SpawnEnemyRoutine());
    }
    
    public void StopSpawningEnemies()
    {
        _spawnEnemies = false;
    }
    
    public void StartSpawningPowerups()
    {
        _spawnPowerups = true;
        StartCoroutine(SpawnPowerupRoutine());
    }
    
    public void StopSpawningPowerups()
    {
        _spawnPowerups = false;
    }
    
    private void InitializePowerupPrefabsDictionary()
    {
        var myObjs = Resources.LoadAll("Prefabs/Powerups", typeof(Powerup));

        //Debug.Log("printing myObs now...");
        foreach (var thisObject in myObjs)
        {
            var powerUp = (Powerup) thisObject;
            var powerupType = powerUp.GetPowerupType();
            var powerupGameObject = powerUp.gameObject;
            _powerupPrefabs.Add(powerupType, powerupGameObject);
            Debug.Log("Powerup Found...  " + thisObject.name);
        }
    }
}