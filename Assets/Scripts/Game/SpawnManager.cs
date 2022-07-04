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
    [SerializeField] private WavesConfig _wavesConfig;
    [SerializeField] private GameObject _enemyContainer;
    [SerializeField] private int _powerupMinSpawnRate = 3;
    [SerializeField] private int _powerupMaxSpawnRate = 7;
    
    [SerializeField] private float _spawnStartDelayTime = 2.0f;
    
    private Dictionary<PowerupType, GameObject> _powerupPrefabs;
    
    private ViewportBounds _viewportBounds;
    
    private bool _spawnEnemies;
    private bool _spawnPowerups;

    private WaitForSeconds _spawnStartDelay;
    private float _zPos = 0f;
    private bool _wavesConfigIsNotNull;

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
            }
        }
    }

    private IEnumerator SpawnAllEnemiesInWave(Wave currentWave)
    {
        if (currentWave is { })
            for (int enemyCount = 0; enemyCount < currentWave.GetNumberOfEnemies(); enemyCount++)
            {
                if (currentWave.HasPath)
                {
                    SpawnPathedEnemies(currentWave);
                }
                else
                {
                    SpawnNonPathedEnemies(currentWave);
                }
                

                yield return new WaitForSeconds(currentWave.GetTimeBetweenSpawns());
            }
    }

    private void SpawnPathedEnemies(Wave currentWave)
    {
        var enemyPrefab = currentWave.GetEnemyPrefab();
        var waypoints = currentWave.GetWaypoints();
        var startingWaypoint = waypoints?.FirstOrDefault();
        if (enemyPrefab != null && startingWaypoint != null)
        {
            var instantiatedEnemy = Instantiate(
                enemyPrefab,
                startingWaypoint.transform.position,
                Quaternion.identity,
                _enemyContainer.transform);

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

    private void SpawnNonPathedEnemies(Wave currentWave)
    {
        var enemyPrefab = currentWave.GetEnemyPrefab();
        if (enemyPrefab != null)
        {
            var xPos = Random.Range(_viewportBounds.Left, _viewportBounds.Right);
            var spawnPosition = new Vector3(xPos, _viewportBounds.Top, _zPos);
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, _enemyContainer.transform);
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
        // TODO: replace rare chance roll with weighted random system in phase II part 6 requirement 
        //max value is exclusive thus we need 101 not 100 to include 100 as a possibility 
        int rollForRarePowerup = Random.Range(1, 101); 
        var rareChance = 75; 
        var powerupEnumValuesList = Enum.GetValues(typeof(PowerupType)).Cast<PowerupType>().ToList();
        if (rollForRarePowerup > rareChance) 
        { 
            var rarePowerupsToRemove = new List<PowerupType> {PowerupType.SprayShot}; 
            powerupEnumValuesList.RemoveAll(p => rarePowerupsToRemove.Contains(p)); 
        } 
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
}