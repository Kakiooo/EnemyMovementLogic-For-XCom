using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelfStatesControl : MonoBehaviour
{
   [HideInInspector] public enum enemyState { Move, Attack, Stay }
    public enemyState _currentState;
    enemyState[] _stateArray = new enemyState[2] { enemyState.Move, enemyState.Attack };
    List<enemyState> _statePool;
    EnemyMovement _refToE_Movement;
    EnemyCastSpell _ref_ECastSpell;
    EnemyHealth _refToHealth;
    float StateIntervalTime, enemyRangeRadius;
    GameObject _refToPlayer;
    public bool isInRange;
    public string enemyType;
    private void Awake()
    {
        enemyRangeRadius = 10;
        _refToPlayer = GameObject.FindGameObjectWithTag("Player");
        _refToE_Movement =GetComponent<EnemyMovement>();
        _ref_ECastSpell=GetComponent<EnemyCastSpell>();
        _refToHealth=GetComponent<EnemyHealth>();
        _statePool = new List<enemyState>();
        StateIntervalTime = 1;
    }

    private void Update()
    {
        EnemyRangeCheck();
        switch (_currentState)
        {
            case enemyState.Move:
                if (enemyType == "NormalEnemy")
                {
                    _refToE_Movement.TimeToMove();
                    //_refToE_Movement.EnemyFreeze();
                }
                break;
            case enemyState.Attack:
                EnemyAttack();
                break;
            case enemyState.Stay:
                EnemyStay();
                break;
        }
    }
    void EnemyRangeCheck() // also used to help determind enemy will attack player or not
    {
        //check distance between player and enemy
        Vector2 disCheck = new Vector2(_refToPlayer.transform.position.x, _refToPlayer.transform.position.z) - new Vector2(transform.position.x, transform.position.z);
        if (disCheck.magnitude <= enemyRangeRadius)
        {
            //keep the distance and recieve retreat function
            print("Entered???" + isInRange);
            isInRange = true;
        }
        else
        {
            isInRange = false;
        }

    }
    void EnemyAttack()
    {
        transform.GetComponent<Renderer>().material.color = Color.green;
        if (_ref_ECastSpell._isAttackEnd)
        {
            StateIntervalTime -= Time.deltaTime;
            if(StateIntervalTime < 0)
            {
                //_currentState = _stateArray[UnityEngine.Random.Range(0, _stateArray.Length)];
                print("Enter Is attack end");
                _ref_ECastSpell._isAttackEnd = false;
                if (gameObject.tag != "TurretEnemy")
                {
                    _currentState = enemyState.Move;
                }
                else { _currentState = enemyState.Attack; }
                StateIntervalTime = 3;
            }

        }
    }
    void EnemyStay()
    {
        transform.position = transform.position;

    }

    void ActionChances()
    {
        // less hp, more likely to hide
        int moveChance = 0;
        int attackChance = 0;
        int hideChance = 0;

        if (_refToHealth._health >= 80)
        {
            moveChance = 5;
            attackChance = 4;
            hideChance = 1;
        }
        else if (_refToHealth._health >= 30 && _refToHealth._health < 80)
        {
            _statePool.Clear();
            moveChance = 2;
            attackChance = 6;
            hideChance = 2;
        }
        else if (_refToHealth._health < 30)
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
            _statePool.Add(enemyState.Stay);
        }
    }
}
