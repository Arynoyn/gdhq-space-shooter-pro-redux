using System;
using System.Linq;
using UnityEngine;

public class PlayerSeeker : MonoBehaviour
{
    [SerializeField] [Range(1, 20)] private float _range = 5;
    [SerializeField] private float _seekingSpeed = 8.0f;
    [SerializeField] private SeekingBehavior _seekingBehavior;
    private float _normalSpeed = 0f;
    private Enemy _enemy;
    private Renderer _enemyRenderer;
    private bool _isEnemyRendererNotNull;
    private bool _isEnemyNotNull;
    private bool _isRamming;
    private bool _activateRamming;
    

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _isEnemyNotNull = _enemy != null;

        if (_isEnemyNotNull)
        {
            _normalSpeed = _enemy.GetMovementSpeed();
            if(_enemy.TryGetComponent(out Renderer enemyRenderer))
            {
                _enemyRenderer = enemyRenderer;
            }
        }

        _isEnemyRendererNotNull = _enemyRenderer != null;
    }

    private void Update()
    {
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, _range);
        Player player = null;
        
        //Check to see if any of the found collider objects are players and store the player in the above variable if found
        bool _isInDistance = colliderArray.Any(c => c.TryGetComponent(out player));
        
        if (_isInDistance)
        {
            switch (_seekingBehavior)
            {
                case SeekingBehavior.Ramming:
                    if (_activateRamming)
                    {
                        PointTowardsTarget(player);
                        if (_isEnemyRendererNotNull)
                        {
                            _enemyRenderer.material.color = Color.red;
                        }
                        _activateRamming = false;
                    }
                    break;
                case SeekingBehavior.Homing:
                    PointTowardsTarget(player);
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
                    if (_isEnemyRendererNotNull)
                    {
                        _enemyRenderer.material.color = Color.white;
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
        float angle = 0;
        _enemy.SetMovementSpeed(_normalSpeed);
        _enemy.SetTrajectoryAngle(angle);
        
    }

    private void PointTowardsTarget(Player player)
    {
        float targetAngleDirectionX = _enemy.transform.position.x - player.transform.position.x;
        float targetAngleDirectionY = _enemy.transform.position.y - player.transform.position.y;
        float angle = Mathf.Atan2(targetAngleDirectionY, targetAngleDirectionX) * Mathf.Rad2Deg;
        angle -= 90; // shift 
        _enemy.SetMovementSpeed(_seekingSpeed);
        _enemy.SetTrajectoryAngle(angle);
        
    }
}