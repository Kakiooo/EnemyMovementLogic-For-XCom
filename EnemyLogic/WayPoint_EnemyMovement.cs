using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WayPoint_EnemyMovement : MonoBehaviour
{
    NavMeshAgent _e_Agent;
    [SerializeField]List<Transform> waypoints = new List<Transform>();
    int _index;
    EnemySelfStatesControl _selfStateControl;
    private void Awake()
    {
        _e_Agent=GetComponent<NavMeshAgent>();
        _selfStateControl=GetComponent<EnemySelfStatesControl>();
    }
    private void Update()
    {
        if (_selfStateControl._currentState == EnemySelfStatesControl.enemyState.Attack)
        {
            _e_Agent.isStopped = true;
        }else if(_selfStateControl._currentState == EnemySelfStatesControl.enemyState.Move)
        {
            _e_Agent.isStopped = false;
            FollowWayPoints();
        }

    }

    void FollowWayPoints()
    {
        _e_Agent.SetDestination(waypoints[_index].transform.position);
        if (!_e_Agent.pathPending && _e_Agent.remainingDistance <= _e_Agent.stoppingDistance)
        {
            _selfStateControl._currentState = EnemySelfStatesControl.enemyState.Attack;//set the next state of waypoint enemy
            _index++;
            if(_index >= waypoints.Count) { _index = 0; }
        }
    }
}
