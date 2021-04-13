#region

using System.Collections;
using UnityEngine;

#endregion

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private float _spawnRate = 5.0f;
    [SerializeField] private GameObject _enemyPrefab;
    private float _screenLimitLeft = -8f;
    private float _screenLimitRight = 8f;
    private float _screenLimitTop = 8.0f;
    private float _zPos = 0f;

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        
        while (true)
        {
            var xPos = Random.Range(_screenLimitLeft, _screenLimitRight);
            var spawnPosition = new Vector3(xPos, _screenLimitTop, _zPos);
            Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
            yield return new WaitForSeconds(_spawnRate);
        }
    }
}