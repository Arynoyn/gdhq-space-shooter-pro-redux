#region

using System.Collections;
using UnityEngine;

#endregion

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private float _spawnRate = 5.0f;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _enemyContainer;
    
    private float _screenLimitLeft = -8f;
    private float _screenLimitRight = 8f;
    private float _screenLimitTop = 8.0f;
    private float _zPos = 0f;
    
    private bool _spawnEnemies = true;


    private void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (_spawnEnemies)
        {
            float xPos = Random.Range(_screenLimitLeft, _screenLimitRight);
            Vector3 spawnPosition = new Vector3(xPos, _screenLimitTop, _zPos);
            Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity, _enemyContainer.transform);
            yield return new WaitForSeconds(_spawnRate);
        }
    }
    
    public void StopSpawningEnemies()
    {
        _spawnEnemies = false;
    }
}