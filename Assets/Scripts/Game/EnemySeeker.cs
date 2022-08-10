using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemySeeker : MonoBehaviour
{
    [SerializeField] [Range(1, 20)] private float _range = 5;
    [SerializeField] private float _seekingSpeed = 8.0f;
    [SerializeField] private SeekingBehavior _seekingBehavior;
    private float _normalSpeed = 0f;
    private float _normalAngle = 0f;
    private Laser _laser;
    private Renderer _laserRenderer;
    private bool _isLaserRendererNotNull;
    private bool _isLaserNotNull;
    private bool _isRamming;
    private bool _activateRamming;
    

    private void Awake()
    {
        _laser = GetComponent<Laser>();
        _isLaserNotNull = _laser != null;

        if (_isLaserNotNull)
        {
            _normalSpeed = _laser.GetMovementSpeed();
            _normalAngle = _laser.GetAngle();
            if(_laser.TryGetComponent(out Renderer laserRenderer))
            {
                _laserRenderer = laserRenderer;
            }
        }

        _isLaserRendererNotNull = _laserRenderer != null;
    }

    private void Update()
    {
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, _range);
        
        GameObject enemy = null;
        
        //Check to see if any of the found collider objects are enemies
        bool _isInDistance = colliderArray.Any(c => c.CompareTag("Enemy"));
        
        if (_isInDistance)
        {
            //Select the GameObjects for all colliders that are tagged "Enemy" and store them in an array
            GameObject[] enemies = colliderArray
                .Where(c => c.CompareTag("Enemy"))
                .Select(c => c.gameObject)
                .ToArray();

            enemy = FindClosestTarget(enemies);
            switch (_seekingBehavior)
            {
                case SeekingBehavior.Ramming:
                    if (_activateRamming)
                    {
                        PointTowardsTarget(enemy.transform);
                        if (_isLaserRendererNotNull)
                        {
                            _laserRenderer.material.color = Color.red;
                        }
                        _activateRamming = false;
                    }
                    break;
                case SeekingBehavior.Homing:
                    PointTowardsTarget(enemy.transform);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
           
            
        }
        else
        {
            switch (_seekingBehavior)
            {
                case SeekingBehavior.Ramming:
                    PointTowardsBottomOfScreen();
                    if (_isLaserRendererNotNull)
                    {
                        _laserRenderer.material.color = Color.white;
                    }
                    _activateRamming = true;
                    break;
                case SeekingBehavior.Homing:
                    PointTowardsBottomOfScreen();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }

    private void PointTowardsBottomOfScreen()
    {
        _laser.SetMovementSpeed(_normalSpeed);
        _laser.SetTrajectoryAngle(_normalAngle);
    }
    
    private GameObject FindClosestTarget(ICollection<GameObject> enemies)
    {
        if (enemies.Count > 0)
        {
            GameObject closestEnemy = enemies.First();
            float? nearestDistance = null;
            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (nearestDistance == null || distance < nearestDistance)
                {
                    nearestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }

        LogError("No Enemies found!");
        return null;
    }

    private void PointTowardsTarget(Transform enemy)
    {
        float targetAngleDirectionX = _laser.transform.position.x - enemy.position.x;
        float targetAngleDirectionY = _laser.transform.position.y - enemy.position.y;
        float angle = Mathf.Atan2(targetAngleDirectionY, targetAngleDirectionX) * Mathf.Rad2Deg;
        angle -= 90; // shift 
        _laser.SetMovementSpeed(_seekingSpeed);
        _laser.SetTrajectoryAngle(angle);
        
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