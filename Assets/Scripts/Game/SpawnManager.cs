#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

#endregion

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Wave Spawning")]
    [SerializeField] private WavesConfig _wavesConfig;
    [SerializeField] private GameObject _enemyContainer;
    [SerializeField] private Wave _bossWave;
    
    [Header("Powerup Spawning")]
    [SerializeField] private int _powerupMinSpawnRate = 3;
    [SerializeField] private int _powerupMaxSpawnRate = 7;
    
    [SerializeField] private float _spawnStartDelayTime = 2.0f;
    [SerializeField] private List<PowerupWeight> _powerupWeights;
    
    private Dictionary<PowerupType, GameObject> _powerupPrefabs;
    
    private bool _spawnEnemies;
    private bool _spawnPowerups;

    private WaitForSeconds _spawnStartDelay;
    private float _zPos = 0f;
    private bool _wavesConfigIsNotNull;
    
    private ViewportBounds _viewportBounds;
    private List<GameObject> _allEnemies = new List<GameObject>();

    private void Start()
    {
        _wavesConfigIsNotNull = _wavesConfig != null;
        if (!_wavesConfigIsNotNull)
        {
            Debug.LogError("WaveConfig is NULL on SpawnManager!");
        }
        
        if (GameManager.Instance == null) { Debug.LogError("Game Manager is NULL on SpawnManager!"); }
        else
        {
            _viewportBounds = GameManager.Instance.GetViewportBounds();
            if (_viewportBounds == null)
            {
                Debug.LogError("Viewport Bounds is NULL on SpawnManager!");
            }
        }
        
        _powerupPrefabs = new Dictionary<PowerupType, GameObject>();
        InitializePowerupPrefabsDictionary();
        if (_powerupPrefabs != null)
        {
            if (_powerupPrefabs.Any())
            {
                InitializePowerupWeightsTable();
            }
            else
            {
                Debug.LogWarning("No powerups in array on SpawnManager");
            }
        }
        else
        {
            Debug.LogError("Powerups array on SpawnManager is NULL");
        }

        _spawnStartDelay = new WaitForSeconds(_spawnStartDelayTime);
    }

    private IEnumerator SpawnEnemyWavesRoutine()
    {
        yield return _spawnStartDelay;

        while (_spawnEnemies)
        {
            if (!_wavesConfigIsNotNull) { continue; }
            List<Wave> waves = _wavesConfig.GetWaves().ToList();
            if (waves.Any())
            {
                for (int currentWaveIndex = _wavesConfig.GetStartingWaveIndex(); currentWaveIndex < waves.Count; currentWaveIndex++)
                {
                    Wave currentWave = waves[currentWaveIndex];
                    yield return StartCoroutine(SpawnAllEnemiesInWave(currentWave));
                    yield return new WaitForSeconds(_wavesConfig.GetTimeBetweenWaves());
                }

                _spawnEnemies = false;
                StartCoroutine(SpawnBossCoroutine());
            }
        }
    }

    private IEnumerator SpawnBossCoroutine()
    {
        while (_allEnemies.Any(e => e != null))
        {
            yield return new WaitForFixedUpdate();
        }
        
        yield return StartCoroutine(SpawnAllEnemiesInWave(_bossWave, false));
    }

    private IEnumerator SpawnAllEnemiesInWave(Wave currentWave, bool randomizeHorizontalPosition = true)
    {
        if (currentWave is { })
        {
            for (int enemyCount = 0; enemyCount < currentWave.GetNumberOfEnemies(); enemyCount++)
            {
                if (currentWave.HasPath)
                {
                    SpawnPathedEnemies(currentWave);
                }
                else
                {
                    SpawnNonPathedEnemies(currentWave, randomizeHorizontalPosition);
                }
                

                yield return new WaitForSeconds(currentWave.GetTimeBetweenSpawns());
            }
        }
    }

    private void SpawnPathedEnemies(Wave currentWave)
    {
        var enemyPrefab = currentWave.GetEnemyPrefab();
        var waypoints = currentWave.GetWaypoints();
        var startingWaypoint = waypoints?.FirstOrDefault();
        if (enemyPrefab is { } && startingWaypoint is { })
        {
            var instantiatedEnemy = Instantiate(
                enemyPrefab,
                startingWaypoint.transform.position,
                Quaternion.identity,
                _enemyContainer.transform);

            _allEnemies.Add(instantiatedEnemy);
            EnemyPathing enemyPathing = instantiatedEnemy.GetComponent<EnemyPathing>();
            if (enemyPathing is { })
            {
                enemyPathing.SetWave(currentWave);
            }
            else
            {
                Debug.LogError(
                    "SpawnManager::SpawnAllEnemiesInWave(119) - Enemy Pathing script is missing from Instantiated Enemy");
            }
        }
    }

    private void SpawnNonPathedEnemies(Wave currentWave, bool randomizeHorizontalPosition = true)
    {
        var enemyPrefab = currentWave.GetEnemyPrefab();
        if (enemyPrefab != null)
        {
            var xPos = randomizeHorizontalPosition 
                ? Random.Range(_viewportBounds.Left, _viewportBounds.Right) 
                : (_viewportBounds.Left + _viewportBounds.Right) / 2;
            var spawnPosition = new Vector3(xPos, _viewportBounds.Top, _zPos);
            var instantiatedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, _enemyContainer.transform);
            _allEnemies.Add(instantiatedEnemy);
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
                var xPos = Random.Range(_viewportBounds.Left, _viewportBounds.Right);
                var spawnPosition = new Vector3(xPos, _viewportBounds.Top, _zPos);
                
                GameObject randomPowerup = GetRandomPowerupPrefab();
                Instantiate(randomPowerup, spawnPosition, Quaternion.identity);
            }
        }
    }
    
    private GameObject GetRandomPowerupPrefab()
    {
        if (_powerupWeights == null)
        {
            LogError("The Powerup Weights collection is NULL");
            return null;
        }

        if (!_powerupWeights.Any())
        {
            LogError("There are no weights in the Powerup Weights collection");
            return null;
        }
        
        if (_powerupWeights.Any())
        {
            var totalRatio = _powerupWeights.Sum(x => x.SpawnWeight);
            var randomValue = Random.Range(0, totalRatio);
            var weightedRandomPowerup =
                _powerupWeights.FirstOrDefault(powerupWeight => (randomValue -= powerupWeight.SpawnWeight) < 0);
            if (weightedRandomPowerup is { })
            {
                if (!_powerupPrefabs.TryGetValue(weightedRandomPowerup.PowerupType, out var randomPowerup))
                {
                    LogError(
                        $"{weightedRandomPowerup.PowerupType.ToString()} does not have a matching prefab in the dictionary", 
                        gameObject);
                    return null;
                }

                return randomPowerup;
            }

            LogError("No matching weighted random powerup.");
        }

        return null;
    }

    public void StartSpawningEnemies()
    {
        _spawnEnemies = true;
        StartCoroutine(SpawnEnemyWavesRoutine());
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
    
    private void InitializePowerupWeightsTable()
    {
        _powerupWeights = _powerupPrefabs.Select(powerup =>
            {
                var powerupComponent = powerup.Value.GetComponent<Powerup>();
                if (powerupComponent is { })
                {
                    return new PowerupWeight
                    {
                        PowerupType = powerup.Key,
                        SpawnWeight = powerupComponent.GetSpawnWeight()
                    };
                }

                return null;
            }).Where(weight => weight != null)
            .ToList();
    }
    
    public void LogError(string message,
        [CallerMemberName] string callingMethod = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        var className = Path.GetFileNameWithoutExtension(callingFilePath);
        Debug.LogError($"{className}::{callingMethod}({callingFileLineNumber}): {message}!");
    }
    
    public void LogError(string message, 
        Object context,
        [CallerMemberName] string callingMethod = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        var className = Path.GetFileNameWithoutExtension(callingFilePath);
        Debug.LogError($"{className}::{callingMethod}({callingFileLineNumber}): {message}!", context);
    }
}