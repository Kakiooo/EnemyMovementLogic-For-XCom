using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class EnemyMovement : MonoBehaviour
{
    EnemyManager _refToEnemyManager;
    EnemySelfStatesControl _refEState;


    [SerializeField] float _fearAttribute = 30, _heathPoint_Enemy = 100;
    GameObject _refToPlayer;
    [SerializeField] float _speed;
    bool _playerIsRight, _playerIsUp, _isInPos;
    public int _current_Index, Num_S;
    bool WithBarricade;
    [HideInInspector] public bool IsRetreat, IsMyTurn, IsChosen, IsInPos, IsActionDone, isInRange, IsOnTheWay;
    Vector3 _sidePos, _former_Dis;
    public float enemyRangeRadius;
    [HideInInspector] public Transform _moveTargetTransform;
    float _maxMoveDis = 30;
    NavMeshAgent _Nav_Enemy;
    EnemyHealth _e_Health;
    EnemyCastSpell _eCastSpell;
    EnemySelfStatesControl _refToStateControlf;


    private void Awake()
    {
        _refEState=GetComponent<EnemySelfStatesControl>();
        _refToPlayer = GameObject.FindGameObjectWithTag("Player");
        _refToEnemyManager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
        _refToStateControlf = GetComponent<EnemySelfStatesControl>();

        _Nav_Enemy = GetComponent<NavMeshAgent>();
        enemyRangeRadius = 20;
        _e_Health = GetComponent<EnemyHealth>();
        _eCastSpell = GetComponent<EnemyCastSpell>();
        IsActionDone = false;
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, Vector3.forward * enemyRangeRadius);
    }


    void TraceDisToPlayer(Vector3 formerPos)
    {
        Vector3 currentDis = _refToPlayer.transform.position - formerPos;
        if (_former_Dis.magnitude < currentDis.magnitude)
        {
            IsRetreat = true;
        }
    }

    public void TimeToMove()
    {
        if (_refToEnemyManager.Turns == Num_S && IsOnTheWay == false)
        {
            if (!IsMyTurn)
            {
                transform.GetComponent<Renderer>().material.color = Color.grey;                        
                SearchAvailableBarricade();
                _Nav_Enemy.destination = _sidePos;
                IsMyTurn = true;
            }
            if (IsMyTurn)
            {
                IsRetreat = false;//reset retreat state and ready for next move
                IsOnTheWay = true;
            }
        }

        if (_refToEnemyManager.Turns == Num_S && IsMyTurn)
        {
            if (!_Nav_Enemy.pathPending && _Nav_Enemy.remainingDistance <= _Nav_Enemy.stoppingDistance)
            {
                IsOnTheWay = false;//know if the trip is end            
                IsMyTurn = false;
                _refEState._currentState = EnemySelfStatesControl.enemyState.Attack;
                //_nextState= _stateArray[UnityEngine.Random.Range(0, _stateArray.Length)];

            }
        }
        //print("is on the way?: " + IsOnTheWay);

    }

    void SearchAvailableBarricade()
    {

        if (!WithBarricade)
        {
            int intendIndex = EnemyChooseBarricade(_refToEnemyManager._barricadeTransform);
            Transform intendObj = _refToEnemyManager._barricadeTransform[intendIndex].transform;
            _sidePos = GetFarthestChildToPlayer(intendObj).position;
            _moveTargetTransform = intendObj;
            _refToEnemyManager._barricadeTransform.Remove(intendObj);
            intendObj.transform.gameObject.GetComponent<BoxCollider>().enabled = false;//get rid of collider to prevent collision between enemy projectile spell and his cover

            WithBarricade = true;
        }
        else
        {
            if (_refToStateControlf.isInRange)
            {
                InRangeEnemyMovement();//when in the range detect if player get closer
            }
            else
            {
                OutRangeEnemyMovement();//when out of range, enemy want to get closer to player
            }

        }

    }


    void InRangeEnemyMovement()
    {
        Transform FormerTransform = _moveTargetTransform;//record former pos
        _former_Dis = _refToPlayer.transform.position - FormerTransform.position;

        List<Transform> DeletePos = new List<Transform>();
        TraceDisToPlayer(_former_Dis);
        //print("Is Retreat?: " + IsRetreat);

        if (IsRetreat && _refToStateControlf.isInRange || _e_Health._health <= _fearAttribute)//decide enemy need to retreat or no
        {

            for (int i = 0; i <= _refToEnemyManager._barricadeTransform.Count - 1; i++)
            {
                Vector3 disCheck = _refToPlayer.transform.position - _refToEnemyManager._barricadeTransform[i].position;
                if (disCheck.magnitude < _former_Dis.magnitude && (transform.position - _refToEnemyManager._barricadeTransform[i].position).magnitude > _maxMoveDis)
                {
                    DeletePos.Add(_refToEnemyManager._barricadeTransform[i]);// (less objects to be calculated using Delete from entire list under this circumstance)
                    _refToEnemyManager._barricadeTransform.Remove(_refToEnemyManager._barricadeTransform[i]);
                }
            }// remove the pos that get closer to player


            int intendIndex = EnemyChooseBarricade(_refToEnemyManager._barricadeTransform);
            Transform intendObj = _refToEnemyManager._barricadeTransform[intendIndex].transform;
            _sidePos = GetFarthestChildToPlayer(intendObj).position;
            _moveTargetTransform = intendObj;
            intendObj.transform.gameObject.GetComponent<BoxCollider>().enabled = false;//get rid of collider to prevent collision between enemy projectile spell and his cover

            for (int i = 0; i <= DeletePos.Count - 1; i++)
            {
                _refToEnemyManager._barricadeTransform.Add(DeletePos[i]);
            }// load back all the avaliable pos for other enemies

            _refToEnemyManager._barricadeTransform.Remove(intendObj);//remove the current barricade that player is on
            
        }
        else
        {
            int intendIndex = EnemyChooseBarricade(_refToEnemyManager._barricadeTransform);
            Transform intendObj = _refToEnemyManager._barricadeTransform[intendIndex].transform;
            intendObj.transform.gameObject.GetComponent<BoxCollider>().enabled = false;//get rid of collider to prevent collision between enemy projectile spell and his cover

            _moveTargetTransform = intendObj;
            _refToEnemyManager._barricadeTransform.Remove(intendObj);

            _sidePos = GetFarthestChildToPlayer(intendObj).position;
        }

        _refToEnemyManager._barricadeTransform.Add(FormerTransform);
        FormerTransform.transform.gameObject.GetComponent<BoxCollider>().enabled = true;//reset barricade collision

    }
    void OutRangeEnemyMovement()
    {
        Transform FormerTransform = _moveTargetTransform;
        Vector2 DisCheck = new Vector2(_refToPlayer.transform.position.x, _refToPlayer.transform.position.z) - new Vector2(FormerTransform.position.x, FormerTransform.position.z);
        float disToPlayer_Former = DisCheck.magnitude;

        List<Transform> GoList = new List<Transform>();
        for (int i = 0; i <= _refToEnemyManager._barricadeTransform.Count - 1; i++)
        {
            Vector2 IgnoreYDisCheck = new Vector2(_refToPlayer.transform.position.x, _refToPlayer.transform.position.z) - new Vector2(_refToEnemyManager._barricadeTransform[i].position.x, _refToEnemyManager._barricadeTransform[i].position.z);
            float disBetweenPnB = IgnoreYDisCheck.magnitude;

            if (disBetweenPnB <= disToPlayer_Former && (transform.position - _refToEnemyManager._barricadeTransform[i].position).magnitude <= _maxMoveDis)
            {
                GoList.Add(_refToEnemyManager._barricadeTransform[i]);//adding the possible barricade for enemy to get closer (less objects to be calculated to create new list of available closer barricade under this circumstance)

            }
        }

        if (GoList.Count > 0)
        {

            int intendIndex = UnityEngine.Random.Range(0, GoList.Count - 1);//choose the random closer barricade
            Transform intendObj = GoList[intendIndex].transform;
            _sidePos = GetFarthestChildToPlayer(intendObj).position;
            _moveTargetTransform = intendObj;
            _refToEnemyManager._barricadeTransform.Remove(intendObj);//occupied the place
            intendObj.transform.gameObject.GetComponent<BoxCollider>().enabled = false;//get rid of collider to prevent collision between enemy projectile spell and his cover
            GoList.Clear();
        }
        else
        {
            int intendIndex = EnemyChooseBarricade(_refToEnemyManager._barricadeTransform);
            Transform intendObj = _refToEnemyManager._barricadeTransform[intendIndex].transform;
            _sidePos = GetFarthestChildToPlayer(intendObj).position;
            _moveTargetTransform = intendObj;
            _refToEnemyManager._barricadeTransform.Remove(intendObj);//occupied the place
            intendObj.transform.gameObject.GetComponent<BoxCollider>().enabled = false;//get rid of collider to prevent collision between enemy projectile spell and his cover
        }

        _refToEnemyManager._barricadeTransform.Add(FormerTransform);
        FormerTransform.transform.gameObject.GetComponent<BoxCollider>().enabled = true;//reset barricade collision
    }


    int EnemyChooseBarricade(List<Transform> barricadeLists)
    {
        //choose to get to the closest barricade
        List<float> dis = new List<float>();
        dis.Clear();
        int index = 0;
        foreach (Transform pos in barricadeLists)
        {
            dis.Add((pos.position - transform.position).magnitude);
        }
        float[] disCheck = dis.ToArray();
        float closetB = Mathf.Min(disCheck);
        for (int i = 0; i <= disCheck.Length - 1; i++)
        {
            if (disCheck[i] == closetB)
            {
                index = i;
            }
        }
        return index;//return the index of barricade that is occupied
    }
    Transform GetFarthestChildToPlayer(Transform x)
    {
        Transform furthestChild = null;
        float furthestDistance = -1;

        foreach (Transform child in x)
        {
            float distance = Vector3.Distance(_refToPlayer.transform.position, child.position);
            if (furthestChild == null)
            {
                furthestChild = child;
                furthestDistance = distance;
            }

            if (distance > furthestDistance)
            {
                furthestDistance = distance;
                furthestChild = child;
            }
        }
        return furthestChild;
    }

    public void EnemyFreeze()
    {
        if (_refToEnemyManager.IsFrozen)
        {
            _Nav_Enemy.isStopped = true;//when player is drawing spell, enemy will stop moving
            return;
        }
        else
        {
            _Nav_Enemy.isStopped = false;//reset enemy back to moving
        }
    }
}
