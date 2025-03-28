using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyManager : MonoBehaviour
{
    //help to prevent enemy stay in the same position
    private GameObject[] _originalBarricadeObjects;
    public List<Transform> _barricadeTransform = new List<Transform>();
    public List<int> indexList = new List<int>();
    public List<int> CurrentIndexList = new List<int>();
    public int Turns = -1;
    private PlayerMovement _refToP;
    [HideInInspector] public bool IsFrozen;
    Transform formerPos;
    float IntervalTimer;
    bool IsFirstRound=true;
    

    private void Awake()
    {
        IntervalTimer = 1;
        _originalBarricadeObjects = GameObject.FindGameObjectsWithTag("Barricade");
        Barricade_Assign();
        _refToP = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        AssignChildNum();
    }
    private void Start()
    {
        _barricadeTransform.Remove(_refToP.TargetObj.transform);
        formerPos = _refToP.TargetObj.transform;
    }

    private void Update()
    {
        if (IsFrozen) return;
        if (Turns > transform.childCount - 1)
        {
            ShuffleChildren();
            Turns = -1;//only trigger this when all enemy finish their former action
            IsFirstRound = false;
        }
        else
        {
            Timer();
        }

        CheckPPos();
        //check if player change pos

    }
    public void AssignChildNum()
    {
        for (int i = 0; i <= transform.childCount - 1; i++)
        {
            transform.GetChild(i).GetComponent<EnemyMovement>().Num_S = i;
        }
    }

    public void ShuffleChildren()
    {
        List<Transform> Children = new List<Transform>();
        for (int i = 0; i <= transform.childCount - 1; i++)
        {
            Children.Add(transform.GetChild(i));
        }
        for (int i =Children.Count - 1;i >= 0; i--)
        {
            int ramdonIndex=Random.Range(0, i+1);
            var tempChild = Children[i];
            Children[i]= Children[ramdonIndex];
            Children[ramdonIndex]=tempChild;//switching position of two children in the list
        }
        for(int i = 0;i <= Children.Count-1; i++)
        {
            Children[i].SetSiblingIndex(i);//this method is used to change the index of children,so that the order of children will be changed
        }
        for (int i = 0; i <= transform.childCount - 1; i++)
        {
            transform.GetChild(i).GetComponent<EnemyMovement>().Num_S = i;
        }

    }

    public void Timer()
    {
        IntervalTimer -= Time.deltaTime;

        if (IntervalTimer < 0)
        {
            Turns++;
            if (IsFirstRound)
            {
                IntervalTimer = 1;
            }
            else
            {
                IntervalTimer = Random.Range(1, 3);
            }

        }
    }

    //approach 
    public void Barricade_Assign()
    {
        //_barricadesPosition.Clear();
        for (int i = 0; i <= _originalBarricadeObjects.Length - 1; i++)
        {
            _barricadeTransform.Add(_originalBarricadeObjects[i].transform);
        }// load in all the barricades in the scene
    }


    public void CheckPPos()
    {
        _barricadeTransform.Remove(_refToP.TargetObj.transform);
        if (formerPos.position == _refToP.TargetObj.transform.position) return;
        _barricadeTransform.Add(formerPos);
        formerPos = _refToP.TargetObj.transform;
    }
    private void OnDisable()
    {
        IsFrozen = true;
    }

    private void OnEnable()
    {
        IsFrozen = false;
    }

}
