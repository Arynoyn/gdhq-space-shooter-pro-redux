#region

using System.Collections;
using System.Linq;
using UnityEngine;

#endregion

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private float _enemySpawnRate = 5.0f;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _enemyContainer;
    [SerializeField] private GameObject[] _powerups;
    [SerializeField] private int _powerupMinSpawnRate = 3;
    [SerializeField] private int _powerupMaxSpawnRate = 7;
    
    private float _screenLimitLeft = -8f;
    private float _screenLimitRight = 8f;
    private float _screenLimitTop = 8.0f;
    private float _zPos = 0f;
    
    private bool _spawnEnemies = true;
    private bool _spawnPowerups = true;


    private void Start()
    {
        if (_powerups == null)
        {
            Debug.LogError("Powerups array on SpawnManager is NULL");
        }
        else if (!_powerups.Any())
        {
            Debug.LogWarning("No powerups in array on SpawnManager");
        }
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnPowerupRoutine());
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

        while (_spawnPowerups)
        {
            int randomSpawnTime = Random.Range(_powerupMinSpawnRate, _powerupMaxSpawnRate + 1);
            yield return new WaitForSeconds(randomSpawnTime);
            if (_powerups.Any())
            {
                int randomPowerupIndex = Random.Range(0, _powerups.Length);
                var xPos = Random.Range(_screenLimitLeft, _screenLimitRight);
                var spawnPosition = new Vector3(xPos, _screenLimitTop, _zPos);
                Instantiate(_powerups[randomPowerupIndex], spawnPosition, Quaternion.identity, _enemyContainer.transform);
            }
        }
    }

    public void StopSpawningEnemies()
    {
        _spawnEnemies = false;
    }
    
    public void StopSpawningPowerups()
    {
        _spawnPowerups = false;
    }
}