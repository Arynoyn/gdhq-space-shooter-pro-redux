#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Wave Spawning")] [SerializeField]
    private WavesConfig _wavesConfig;

    [SerializeField] private GameObject _enemyContainer;

    [Header("Powerup Spawning")] [SerializeField]
    private int _powerupMinSpawnRate = 3;

    [SerializeField] private int _powerupMaxSpawnRate = 7;
    [SerializeField] private float _spawnStartDelayTime = 2.0f;
    [SerializeField] private List<PowerupWeight> _powerupWeights;

    private GameManager _gameManager;

    private Dictionary<PowerupTypeEnum, GameObject> _powerupPrefabs;
    private bool _spawnEnemies;
    private bool _spawnPowerups;
    private WaitForSeconds _spawnStartDelay;
    private ViewportBounds _viewportBounds;
    private bool _wavesConfigIsNotNull;
    private float _zPos = 0f;

    private void Start()
    {
        _wavesConfigIsNotNull = _wavesConfig != null;
        if (!_wavesConfigIsNotNull)
        {
            Debug.LogError("WaveConfig is NULL on SpawnManager!");
        }

        _gameManager = FindObjectOfType<GameManager>();
        if (_gameManager == null)
        {
            Debug.LogError("Game Manager is NULL on SpawnManager!");
        }
        else
        {
            _viewportBounds = _gameManager.GetViewportBounds();
            if (_viewportBounds == null)
            {
                Debug.LogError("Viewport Bounds is NULL on SpawnManager!");
            }
        }

        _powerupPrefabs = new Dictionary<PowerupTypeEnum, GameObject>();
        InitializePowerupPrefabsDictionary();
        if (_powerupPrefabs is { })
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
            if (!_wavesConfigIsNotNull)
            {
                continue;
            }

            var waves = _wavesConfig.GetWaves().ToList();
            if (waves.Any())
            {
                for (var currentWaveIndex = _wavesConfig.GetStartingWaveIndex();
                    currentWaveIndex < waves.Count;
                    currentWaveIndex++)
                {
                    var currentWave = waves[currentWaveIndex];
                    yield return StartCoroutine(SpawnAllEnemiesInWave(currentWave));
                    yield return new WaitForSeconds(_wavesConfig.GetTimeBetweenWaves());
                }
            }
        }
    }

    private IEnumerator SpawnAllEnemiesInWave(Wave currentWave)
    {
        if (currentWave is { })
        {
            for (var enemyCount = 0; enemyCount < currentWave.GetNumberOfEnemies(); enemyCount++)
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

            var enemyPathing = instantiatedEnemy.GetComponent<EnemyPathing>();
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
        if (enemyPrefab is { })
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
            var randomSpawnTime = Random.Range(_powerupMinSpawnRate, _powerupMaxSpawnRate + 1);
            yield return new WaitForSeconds(randomSpawnTime);
            if (_powerupPrefabs.Any())
            {
                var xPos = Random.Range(_viewportBounds.Left, _viewportBounds.Right);
                var spawnPosition = new Vector3(xPos, _viewportBounds.Top, _zPos);

                var randomPowerup = GetRandomPowerupPrefab();
                if (randomPowerup is { })
                {
                    Instantiate(randomPowerup, spawnPosition, Quaternion.identity);
                }
            }
        }
    }

    private GameObject GetRandomPowerupPrefab()
    {
        if (_powerupWeights is { } && _powerupWeights.Any())
        {
            var totalRatio = _powerupWeights.Sum(x => x.SpawnWeight);
            var randomValue = Random.Range(0, totalRatio);
            var weightedRandomPowerup =
                _powerupWeights.FirstOrDefault(powerupWeight => (randomValue -= powerupWeight.SpawnWeight) < 0);
            if (weightedRandomPowerup is { })
            {
                if (!_powerupPrefabs.TryGetValue(weightedRandomPowerup.PowerupType, out var randomPowerup))
                {
                    Debug.LogError(
                        $"SpawnManager::GetRandomPowerupPrefab(182): {weightedRandomPowerup.PowerupType.ToString()} does not have a matching prefab in the dictionary", 
                        gameObject);
                    return null;
                }

                return randomPowerup;
            }

            Debug.LogError("SpawnManager::GetRandomPowerupPrefab(186): No matching weighted random powerup.");
        }

        if (_powerupWeights == null)
        {
            Debug.LogError("SpawnManager::GetRandomPowerupPrefab(192): The Powerup Weights collection is NULL");
        }

        if (!_powerupWeights.Any())
        {
            Debug.LogError(
                "SpawnManager::GetRandomPowerupPrefab(199): There are no weights in the Powerup Weights collection");
        }

        return null;
    }

    public void StopSpawningEnemies()
    {
        _spawnEnemies = false;
    }

    public void StartSpawningEnemies()
    {
        _spawnEnemies = true;
        StartCoroutine(SpawnEnemyWavesRoutine());
    }

    public void StopSpawningPowerups()
    {
        _spawnPowerups = false;
    }

    public void StartSpawningPowerups()
    {
        _spawnPowerups = true;
        StartCoroutine(SpawnPowerupRoutine());
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
}