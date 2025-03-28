using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Collider_SC_Enemy : MonoBehaviour
{
    float _DamageOffsetTime, _SpellDmgToCovers;
    public float _SpellDmgToEnemies;
    public TutorialEnd _endSc;
    private void Awake()
    {
        _endSc = GameObject.Find("EnemyManager").GetComponent<TutorialEnd>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.GetComponent<PlayerHealth>().TakeDamage_P_Collider(_SpellDmgToEnemies);
            print("Hit Player");
        }
        if (other.gameObject.tag == "Barricade")
        {
            other.GetComponent<CoverDamageSC>().TakeDamage(_SpellDmgToCovers, _DamageOffsetTime);
        }
    }
    private void OnDestroy()
    {
        if (SceneManager.GetActiveScene().name == "RepositionTutorial") _endSc.shootTimes++;
    }
}
