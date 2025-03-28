using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Projectile_Spell_Enemy : MonoBehaviour
{
    [HideInInspector] public Vector3 Shooter, GetDamager;
    Vector3 shootDir;
    [HideInInspector] public float Impulse;
    [SerializeField] EnemySpell _spellData;
    TutorialEnd _endSc;
    Rigidbody _rb;
    PlayerMovement _refP;
    float vanishTimer;
    string SceneName;
    private void Awake()
    {
        SceneName = SceneManager.GetActiveScene().name;
        _refP =GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        Impulse =10;
        Shooter =transform.position;
        GetDamager= _refP.transform.position;
        shootDir = (GetDamager - Shooter).normalized;
        if (SceneName == "RepositionTutorial") _endSc = GameObject.Find("EnemyManager").GetComponent<TutorialEnd>();
        _rb =GetComponent<Rigidbody>();
        vanishTimer = 3;
        FireProjectile();
    }
    private void Update()
    {

        vanishTimer-=Time.deltaTime;
        if(vanishTimer < 0)
        {
            Destroy(gameObject);
        }
    }

    public void FireProjectile()
    {
        _rb.AddForce(shootDir * Impulse, ForceMode.Impulse);
        //might change it becomes:::::::: shooting to four dirction but not follow player
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            //when the projectile collides with player deal damage to it

            other.GetComponent<PlayerHealth>().TakeDamage_P(_spellData.Damage, Shooter);
            Destroy(gameObject);
        }
        if (other.tag == "Barricade")
        {
            float coverHP = other.GetComponent<CoverDamageSC>()._coverHP;
            if (coverHP >= 0&&other.gameObject!=null)
            {
                coverHP -= _spellData.Damage;
                other.GetComponent<CoverDamageSC>()._coverHP=coverHP;
            }
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (SceneManager.GetActiveScene().name == "RepositionTutorial") _endSc.shootTimes++;
    }

    public static float EaseInBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return c3 * x * x * x - c1 * x * x;
    }
}
