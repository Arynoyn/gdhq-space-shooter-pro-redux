#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

public class EnemyPathing : MonoBehaviour
{
    private Wave _wave;
    private List<Transform> _waypoints;
    private int _currentWaypointIndex;

    // Start is called before the first frame update
    private void Start()
    {
        _waypoints = _wave.GetWaypoints();
        transform.position = _waypoints[_currentWaypointIndex].transform.position;
    }

    private void Update()
    {
        MoveTowardsWaypoint();
    }
    
    public void SetWave(Wave wave) => _wave = wave;

    private void MoveTowardsWaypoint()
    {
        if (_currentWaypointIndex < _waypoints.Count)
        {
            var targetWaypointPosition = _waypoints[_currentWaypointIndex].transform.position;
            var moveDistance = _wave.GetMovementSpeed() * Time.deltaTime;
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetWaypointPosition,
                moveDistance);

            if (transform.position == targetWaypointPosition) _currentWaypointIndex++;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}