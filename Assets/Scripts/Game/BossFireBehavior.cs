using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BossFireBehavior : MonoBehaviour, IFireLaserBehavior
{
    // Attack Properties
    [Header("Attacks")]
    [Space]
    [SerializeField] private AudioClip _laserSound;
    [SerializeField] private GameObject _laserPrefab;
    private Vector3 _laserOffset = new Vector3(0, -1.05f, 0);
    private bool _fireActive = true;
    private IEnumerator _fireLaserRoutine;
    
    private AudioSource _audioSource;
    private Player[] _players;

    // Start is called before the first frame update
    void Start()
    {
        _players = FindObjectsOfType<Player>();
        if (_laserPrefab == null) { LogError("LaserPrefab is NULL on EnemyFireLaser!"); }
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) { LogError("Audio Source is missing on Enemy!"); }
        if (_laserSound == null) { LogError("Laser Sound missing from EnemyFireLaser!"); }
    }
    
    public void DisableFiring()
    {
        _fireActive = false;
    }

    public void Fire(Transform target)
    {
        if(_fireActive) {
            float angleToTarget = CalculateFiringAngle(target);
            FireLaser(angleToTarget);
        } 
    }

    public void FireAtClosestTarget()
    {
        if(_fireActive) {
            Transform closestTarget = FindClosestTarget();
            Fire(closestTarget);
        }
    }
    
    private void FireLaser(float angleToTarget)
    {
        Laser laser = _laserPrefab.GetComponent<Laser>();
        laser.SetTrajectoryAngle(angleToTarget);
        Instantiate(_laserPrefab,  transform.position + _laserOffset, Quaternion.identity);
        
        _audioSource.PlayOneShot(_laserSound);
    }
    
    private float CalculateFiringAngle(Transform closestTarget)
    {
        var position = transform.position;
        var targetPosition = closestTarget.transform.position;
        float targetAngleDirectionX = targetPosition.x - position.x;
        float targetAngleDirectionY = targetPosition.y - position.y;
        float angle = Mathf.Atan2(targetAngleDirectionY, targetAngleDirectionX) * Mathf.Rad2Deg;
        angle -= 90; // shift 
        return angle;
    }
    
    private Transform FindClosestTarget()
    {
        if (_players.Length > 0)
        {
            Player closestPlayer = _players[0];
            float? nearestDistance = null;
            foreach (Player player in _players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (nearestDistance == null || distance < nearestDistance)
                {
                    nearestDistance = distance;
                    closestPlayer = player;
                }
            }

            return closestPlayer.transform;
        }

        LogError("No Players found!");
        return transform;
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
