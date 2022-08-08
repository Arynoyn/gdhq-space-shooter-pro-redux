using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyDodgeLaserBehavior : MonoBehaviour
{
    [SerializeField] private float _dodgeSpeed = 8.0f;
    [SerializeField, Range(0.5f, 10.0f)]private float _range = 2.5f;
    private Enemy _enemy;
    private bool _isEnemyNotNull;
    private float _normalSpeed;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _isEnemyNotNull = _enemy != null;

        if (_isEnemyNotNull)
        {
            _normalSpeed = _enemy.GetMovementSpeed();
        }
    }

    void Update()
    {
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, _range);
        bool _isInDistance = colliderArray.Any(c =>
        {
            Laser laser = c.GetComponent<Laser>();
            return laser != null && laser.GetOwnerType() == LaserType.Player;
        });
        if (_isInDistance)
        {
            Laser[] lasers = colliderArray
                .Where(c => {
                    Laser laser = c.GetComponent<Laser>();
                    return laser != null && laser.GetOwnerType() == LaserType.Player;
                })
                .Select(c => c.GetComponent<Laser>())
                .ToArray();
            Laser closestLaser = FindClosestTarget(lasers);
            if (closestLaser != null)
            {
                PointAwayFromTarget(closestLaser);
            }
        }
        else
        {
            PointTowardsBottomOfScreen();
        }
    }
    
    private void PointTowardsBottomOfScreen()
    {
        float angle = 0;
        _enemy.SetMovementSpeed(_normalSpeed);
        _enemy.SetTrajectoryAngle(angle);
        
    }
    
    private Laser FindClosestTarget(Laser[] lasers)
    {
        if (lasers.Length > 0)
        {
            Laser closestLaser = lasers[0];
            float? nearestDistance = null;
            foreach (Laser laser in lasers)
            {
                float distance = Vector3.Distance(transform.position, laser.transform.position);
                if (nearestDistance == null || distance < nearestDistance)
                {
                    nearestDistance = distance;
                    closestLaser = laser;
                }
            }

            return closestLaser;
        }

        LogError("No Lasers found!");
        return null;
    }

    private void PointAwayFromTarget(Laser laser)
    {
        float targetAngleDirectionX = laser.transform.position.x - _enemy.transform.position.x;
        float targetAngleDirectionY = laser.transform.position.y - _enemy.transform.position.y;
        float angle = Mathf.Atan2(targetAngleDirectionY, targetAngleDirectionX) * Mathf.Rad2Deg;
        angle -= 90; // shift 
        _enemy.SetMovementSpeed(_dodgeSpeed);
        _enemy.SetTrajectoryAngle(angle);
        
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
