#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

public class EnemyPathing : MonoBehaviour
{
    [SerializeField] private GameObject _pathPrefab;
    [SerializeField] private float _movementSpeed = 2f;
    private List<Transform> _waypoints;
    private int _currentWaypointIndex;

    // Start is called before the first frame update
    private void Start()
    {
        _waypoints = _pathPrefab.transform.Cast<Transform>().ToList();
        var startingWaypoint = _waypoints[_currentWaypointIndex].transform.position;
        transform.position = startingWaypoint;
    }

    private void Update()
    {
        MoveTowardsWaypoint();
    }

    private void MoveTowardsWaypoint()
    {
        if (_currentWaypointIndex < _waypoints.Count)
        {
            var targetWaypointPosition = _waypoints[_currentWaypointIndex].transform.position;
            var moveDistance = _movementSpeed * Time.deltaTime;
            transform.position =
                Vector2.MoveTowards(transform.position, targetWaypointPosition, moveDistance);

            if (transform.position == targetWaypointPosition) _currentWaypointIndex++;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}