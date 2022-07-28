using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyFireSmartLaser : MonoBehaviour, IFireLaserBehavior
{
    // Attack Properties
    [Header("Attacks")]
    [Space]
    [SerializeField] private AudioClip _laserSound;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private float _fireRate = 0.50f;
    private Vector3 _laserOffset = new Vector3(0, -1.05f, 0);
    private float _nextFireDelay;
    private bool _fireActive = true;
    private IEnumerator _fireLaserRoutine;
    
    private AudioSource _audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        if (_laserPrefab == null) { LogError("LaserPrefab is NULL on EnemyFireLaser!"); }
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) { LogError("Audio Source is missing on Enemy!"); }
        if (_laserSound == null) { LogError("Laser Sound missing from EnemyFireLaser!"); }
        
        _nextFireDelay = Random.Range(_fireRate, _fireRate * 2);
        
        _fireLaserRoutine = FireLaserRoutine();
        StartCoroutine(_fireLaserRoutine);
    }

    public void DisableFiring()
    {
        _fireActive = false;
    }
    
    IEnumerator FireLaserRoutine()
    {
        while (_fireActive)
        {
            yield return new WaitForSeconds(_nextFireDelay);
            FireLaser();
        }
    }
    
    private void FireLaser()
    {
        float targetVerticalPosition = FindTargetVerticalPosition();
        Vector3 position = transform.position;
        bool targetBehindEnemy = targetVerticalPosition - position.y > 0;
        
        Laser laser = _laserPrefab.GetComponent<Laser>();
        
        float laserOrientationAngle;
        if (targetBehindEnemy)
        {
            laserOrientationAngle = 0f;
            _laserOffset = -_laserOffset;
        }
        else
        {
            laserOrientationAngle = 180f;
        }
        
        laser.SetAngle(laserOrientationAngle);
        Instantiate(_laserPrefab,  position + _laserOffset, Quaternion.identity);
        
        _audioSource.PlayOneShot(_laserSound);
        CalculateNextFireTime();
    }

    private float FindTargetVerticalPosition()
    {
        Player[] players = FindObjectsOfType<Player>();
        if (players.Length > 0)
        {
            Player closestPlayer = players[0];
            float? nearestDistance = null;
            foreach (Player player in players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (nearestDistance == null || distance < nearestDistance)
                {
                    nearestDistance = distance;
                    closestPlayer = player;
                }
            }

            return closestPlayer.transform.position.y - transform.position.y;
        }
        else
        {
            LogError("No Players found!");
            return transform.position.y;
        }
    }

    private void CalculateNextFireTime()
    {
        _nextFireDelay = Random.Range(3.0f, 7.0f);
    }
    
    private static void LogError(string message,
        [CallerMemberName] string callingMethod = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        var className = Path.GetFileNameWithoutExtension(callingFilePath);
        Debug.LogError($"{className}::{callingMethod}({callingFileLineNumber}): {message}!");
    }
   
    private static void LogWarning(string message,
        [CallerMemberName] string callingMethod = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        var className = Path.GetFileNameWithoutExtension(callingFilePath);
        Debug.LogWarning($"{className}::{callingMethod}({callingFileLineNumber}): {message}!");
    }
}
