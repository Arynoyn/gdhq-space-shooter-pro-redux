using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyFireLaser : MonoBehaviour, IFireLaserBehavior
{
    // Attack Properties
    [Header("Attacks")]
    [Space]
    [SerializeField] private AudioClip _laserSound;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private float _fireRate = 0.50f;
    private readonly Vector3 _laserOffset = new Vector3(0, -1.05f, 0);
    private float _nextFireDelay;
    private bool _fireActive = true;
    private IEnumerator _fireLaserRoutine;
    private float downAngle = 180f;
    
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

    public void Fire(Transform target) { FireLaser(downAngle); }
    public void FireAtClosestTarget() { FireLaser(downAngle); }

    IEnumerator FireLaserRoutine()
    {
        while (_fireActive)
        {
            yield return new WaitForSeconds(_nextFireDelay);
            FireLaser(downAngle);
            CalculateNextFireTime();
        }
    }
    
    private void FireLaser(float angleToTarget)
    {
        Laser laser = _laserPrefab.GetComponent<Laser>();
        laser.SetTrajectoryAngle(angleToTarget);
        Instantiate(_laserPrefab, transform.position + _laserOffset, Quaternion.identity);
        _audioSource.PlayOneShot(_laserSound);
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
