using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AttackDamageModify : MonoBehaviour
{
    // Start is called before the first frame update

    public LayerMask _barricadeLayer;
    Vector2 Range = new Vector2(10, 30);
    Ray test;
    private void Awake()
    {

    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if target is hiding behind the cover this damage will be deal to cover

    }

    public (float, int, RaycastHit[]) DamageModify(Vector3 Shooter, Vector3 Target, Vector2 DamageRange)
    {
        //check the distance between target enemy 
        float modify = 0;
        Vector3 DirCheck = Target-Shooter;
        if (DamageRange.x <= DirCheck.magnitude && DirCheck.magnitude <= DamageRange.y)
        {
            modify = DirCheck.magnitude / 5;
        }
        else modify = 0;
        //check how many obstacles is along the way of aiming
        RaycastHit[] hit;
        hit = Physics.RaycastAll(Shooter, DirCheck, DirCheck.magnitude, _barricadeLayer);
        test = new Ray(Shooter, DirCheck);
        int Num_Barricade_InBetween=hit.Length;

        modify -= (hit.Length-1)/2;
        //check player is hiding or no


        //Check Height diff
        float Height=Target.y-Shooter.y;
        if(Height > 0)
        {
            //when target pos is higher than shooter---Lose Privilege
            modify *= 2;
        }
        if (Height <= 0)
        {
            //when target pos is lower than shooter---Got Privilege
            modify /= 2;
        }

        return (modify, Num_Barricade_InBetween,hit);
    }

}
