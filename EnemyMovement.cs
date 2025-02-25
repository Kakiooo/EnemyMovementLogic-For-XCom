using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Xml.Linq;

public class EnemyMovement : MonoBehaviour
{
    EnemyManager _refToEnemyManager;
    [SerializeField] enum enemyState { Move, Attack, Hide, Stay ,Freeze}
    [SerializeField] enemyState _currentState;
    List<enemyState> _statePool;

    [SerializeField] float _fearAttribute = 50, _heathPoint_Enemy = 100;
    GameObject _refToPlayer;
    [SerializeField] float _speed;
    [SerializeField] bool _playerIsRight, _playerIsUp;
    public Slider EnemyHealthBar;
    public int _current_Index, Num_S;
    bool WithBarricade;
    public bool IsRetreat, IsMyTurn, IsChosen;
    Vector3  _sidePos, _former_Dis;
    float enemyRangeRadius,_rangeLimited=20;
    public bool isInRange;

    Transform _moveTargetTransform;

    NavMeshAgent _Nav_Enemy;

    private void Awake()
    {
        _refToPlayer = GameObject.FindGameObjectWithTag("Player");
        _refToEnemyManager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
        _statePool = new List<enemyState>();
        _Nav_Enemy = GetComponent<NavMeshAgent>();
        enemyRangeRadius = 15;
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, Vector3.forward* enemyRangeRadius);
        //EnemyHealthBar.value = _heathPoint_Enemy;//visual feedback of losing health
        switch (_currentState)
        {
            case enemyState.Move:
                TimeToGo();
                break;
            case enemyState.Attack:
                EnemyAttack();
                break;

            case enemyState.Hide:
                EnemyHide();
                break;
            case enemyState.Stay:

                break;
            case enemyState.Freeze:

                break;
        }

        EnemyRangeCheck();
    }

    void TraceDisToPlayer(Vector3 formerPos)
    {
        Vector3 currentDis = _refToPlayer.transform.position - formerPos;
        if (_former_Dis.magnitude < currentDis.magnitude)
        {
            IsRetreat = true;
        }


    }

    void TimeToGo()
    {
        if (_refToEnemyManager.Turns == Num_S)
        {
            if (_currentState == enemyState.Move && !IsMyTurn)
            {
                Invoke("SearchAvailableBarricade", 0);
                IsMyTurn = true;
            }
            if (IsMyTurn)
            {
                IsRetreat = false;//reset retreat state and ready for next move          
                _Nav_Enemy.destination = _sidePos;
            }
        }
        if (_refToEnemyManager.Turns == -1)
        {
            IsMyTurn = false;
        }


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

            WithBarricade = true;
        }
        else 
        {
            if (isInRange)
            {
                InRangeEnemyMovement();//when in the range detect if player get closer
            }
            else
            {
                OutRangeEnemyMovement();//when out of range, enemy want to get closer to player
            }

        }

    }

    void EnemyRangeCheck()
    {
        //check distance between player and enemy
        Vector2 disCheck = new Vector2(_refToPlayer.transform.position.x, _refToPlayer.transform.position.z) - new Vector2(transform.position.x, transform.position.z);
        //print("This is the distance between player And Me" + disCheck.magnitude);
        if (disCheck.magnitude <= enemyRangeRadius)
        {
            //keep the distance and recieve retreat function
            isInRange = true;
        }
        else
        {
            isInRange = false;
            //search for the barricade that will bring enemy closer to player
        }
    }

    void InRangeEnemyMovement()
    {
        Transform FormerTransform = _moveTargetTransform;//record former pos
        _former_Dis = _refToPlayer.transform.position - FormerTransform.position;

        List<Transform> DeletePos = new List<Transform>();
        TraceDisToPlayer(_former_Dis);
        print("Is Retreat?: " + IsRetreat);

        if (IsRetreat && isInRange)//decide enemy need to retreat or no
        {

            for (int i = 0; i <= _refToEnemyManager._barricadeTransform.Count - 1; i++)
            {
                Vector3 disCheck = _refToPlayer.transform.position - _refToEnemyManager._barricadeTransform[i].position;
                if (disCheck.magnitude < _former_Dis.magnitude&&(transform.position - _refToEnemyManager._barricadeTransform[i].position).magnitude > _rangeLimited)
                {
                    DeletePos.Add(_refToEnemyManager._barricadeTransform[i]);// (less objects to be calculated using Delete from entire list under this circumstance)
                    _refToEnemyManager._barricadeTransform.Remove(_refToEnemyManager._barricadeTransform[i]);
                }
            }// remove the pos that get closer to player


            int intendIndex = EnemyChooseBarricade(_refToEnemyManager._barricadeTransform);
            Transform intendObj = _refToEnemyManager._barricadeTransform[intendIndex].transform;
            _sidePos = GetFarthestChildToPlayer(intendObj).position;
            _moveTargetTransform = intendObj;

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

            _moveTargetTransform = intendObj;
            _refToEnemyManager._barricadeTransform.Remove(intendObj);

            _sidePos = GetFarthestChildToPlayer(intendObj).position;
        }

        _refToEnemyManager._barricadeTransform.Add(FormerTransform);
    }
    void OutRangeEnemyMovement()
    {
        Transform FormerTransform = _moveTargetTransform;
        Vector2 IgnoreYAxis = new Vector2(_refToPlayer.transform.position.x, _refToPlayer.transform.position.z) - new Vector2(FormerTransform.position.x, FormerTransform.position.z);
        float disToPlayer = IgnoreYAxis.magnitude;

        List<Transform> GoList = new List<Transform>();
        for (int i = 0; i <= _refToEnemyManager._barricadeTransform.Count - 1; i++)
        {
            Vector2 IgnoreYDisCheck = new Vector2(_refToPlayer.transform.position.x, _refToPlayer.transform.position.z) - new Vector2(_refToEnemyManager._barricadeTransform[i].position.x, _refToEnemyManager._barricadeTransform[i].position.z);
            float disBetweenPnB = IgnoreYDisCheck.magnitude;
            if (disBetweenPnB <= disToPlayer)
            {
                GoList.Add(_refToEnemyManager._barricadeTransform[i]);//adding the possible barricade for enemy to get closer (less objects to be calculated to create new list of available closer barricade under this circumstance)
            }
        }

        int intendIndex = UnityEngine.Random.Range(0, GoList.Count - 1);//choose the random closer barricade
        print(GoList.Count);
        Transform intendObj = GoList[intendIndex].transform;
        _sidePos = GetFarthestChildToPlayer(intendObj).position;
        _moveTargetTransform = intendObj;

        for (int i = 0; i <= GoList.Count - 1; i++)
        {
            GoList.Clear();
        }// load back all the avaliable pos for other enemies

        _refToEnemyManager._barricadeTransform.Remove(intendObj);//occupied the place

        _refToEnemyManager._barricadeTransform.Add(FormerTransform);
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

    void EnemyHide()// increase the chance to not get hit
    {
        transform.GetComponent<Renderer>().material.color = Color.red;
        transform.localScale = new Vector3(3, 1, 1);//using shape changes to indicate enemy is hiding
    }

    void EnemyAttack()
    {
        transform.GetComponent<Renderer>().material.color = Color.green;
    }

  
    void ActionChances()
    {
        // less hp, more likely to hide
        int moveChance = 0;
        int attackChance = 0;
        int hideChance = 0;

        if (_heathPoint_Enemy >= 80)
        {
            moveChance = 5;
            attackChance = 4;
            hideChance = 1;
        }
        else if (_heathPoint_Enemy >= 30 && _heathPoint_Enemy < 80)
        {
            _statePool.Clear();
            moveChance = 2;
            attackChance = 6;
            hideChance = 2;
        }
        else if (_heathPoint_Enemy < 30)
        {
            _statePool.Clear();
            moveChance = 1;
            attackChance = 3;
            hideChance = 6;
        }
        ActionPoolLoadIn(moveChance, attackChance, hideChance);// load the chances for each actions into the pool
    }

    void ActionPoolLoadIn(int moveChanve, int attackChance, int hideChance)
    {
        for (int i = 0; i < moveChanve; i++)
        {
            _statePool.Add(enemyState.Move);
        }
        for (int i = 0; i < attackChance; i++)
        {
            _statePool.Add(enemyState.Attack);
        }
        for (int i = 0; i < hideChance; i++)
        {
            _statePool.Add(enemyState.Hide);
        }
    }



}
