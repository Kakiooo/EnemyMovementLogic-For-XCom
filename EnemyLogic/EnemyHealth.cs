using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EnemyHealth : MonoBehaviour
{
    [SerializeField] public float _health;
    float _easeHealth, _easeSpeed;
    Animator _camera_Animator;
    Canvas _healthBarUI;
    CanvasGroup _a_Control_Canvas;
    Slider _healthSlider, _easeHealthSlider;
    Quaternion _defultRotation;
    AttackDamageModify _refToDamageModify;
    GameObject _refToPlayer;
    EnemyMovement _refToEnemy;
    bool _isDamaged;
    float _ShowHPBar;
    private void Awake()
    {
        _easeSpeed = 0.01f;
        _easeHealth = _health;
        _easeHealthSlider = transform.GetChild(0).transform.GetChild(0).GetComponent<Slider>();
        _healthSlider = transform.GetChild(0).transform.GetChild(1).GetComponent<Slider>();//may causing bug in the future
        _healthSlider.maxValue = _health;
        _healthSlider.value = _health;
        _easeHealthSlider.value = _easeHealth;

        _camera_Animator = GameObject.Find("GameplayStateDrivenCamera").GetComponent<Animator>();
        _healthBarUI = transform.GetChild(0).GetComponent<Canvas>();
        _a_Control_Canvas = _healthBarUI.GetComponent<CanvasGroup>();
        _a_Control_Canvas.alpha = 0;
        _defultRotation = _healthBarUI.transform.rotation;
        _refToDamageModify = GetComponent<AttackDamageModify>();
        _refToPlayer = GameObject.FindGameObjectWithTag("Player");
        _refToEnemy = GetComponent<EnemyMovement>();
    }

    private void Update()
    {
        if (_health != _healthSlider.value)
        {
            _healthSlider.value = _health;
        }

        if (_health < 0)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        HealthBarDisplay();
        FadeEffect(0.5f, _isDamaged);
        EaseMotion(_easeSpeed);
    }

    public void TakeDamage_E(float damage)
    {
        (float modify, int Barricade_between, RaycastHit[] HitArray) = _refToDamageModify.DamageModify(_refToPlayer.transform.position, transform.position, new Vector2(0, 30));
        damage *= modify;

        if (gameObject.tag == "Enemy")
        {
            //if (_refToEnemy._moveTargetTransform != null)
            //{
            //    Transform currentCover = _refToEnemy._moveTargetTransform;
            //    //if cover health is larger than 0 and enemy is hiding then deal damage to cover
            //    if (currentCover.GetComponent<CoverDamageSC>()._coverHP > 0 && Barricade_between >= 0)
            //    {
            //        currentCover.GetComponent<CoverDamageSC>()._coverHP -= damage;// may inplement more things as percentage on hitting 
            //    }
            //    else if (currentCover.GetComponent<CoverDamageSC>()._coverHP <= 0)  //else deal damage to enemy
            //    {
            //        _health -= damage;
            //        _isDamaged = true;
            //        _ShowHPBar = 1.2f;
            //    }
            //}
            //else
            //{
            //    _health -= damage;
            //    _isDamaged = true;
            //    _ShowHPBar = 1.2f;
            //}
            _health -= damage;
            _isDamaged = true;
            _ShowHPBar = 1.2f;
        }//MAY change the logic for this one!!!!!!!!!!!!!!!
        else
        {
            _health -= damage;
            _isDamaged = true;
            _ShowHPBar = 1.2f;
        }

    }
    public void TakeDamage(float damage, float timeOffset)
    {
        StartCoroutine(TakeDamageCoroutine(damage, timeOffset));
    }
    public IEnumerator TakeDamageCoroutine(float damage, float timeOffset)
    {
        yield return new WaitForSeconds(timeOffset);//to sync the feedback animation
        TakeDamage_E(damage);
    }
    void HealthBarDisplay()
    {
        //always facing the camera 90 degree rotation along camera movement
        int currentState = _camera_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (currentState == Animator.StringToHash("Start1"))
        {
            _healthBarUI.transform.rotation = _defultRotation;
        }
        else if (currentState == Animator.StringToHash("2"))
        {
            _healthBarUI.transform.rotation = Quaternion.Euler(45, 90, 0);
        }
        else if (currentState == Animator.StringToHash("3"))
        {
            _healthBarUI.transform.rotation = Quaternion.Euler(45, 180, 0);
        }
        else if (currentState == Animator.StringToHash("4"))
        {
            _healthBarUI.transform.rotation = Quaternion.Euler(45, 270, 0);
        }

    }

    public void FadeEffect(float time, bool IsDamaged)
    {
        if (!IsDamaged)
        {
            _a_Control_Canvas.alpha -= 1 / time * Time.deltaTime;
        }
        if (IsDamaged)
        {
            _a_Control_Canvas.alpha += 1 / time * Time.deltaTime; //Display duration must be larger than time 
            _ShowHPBar -= Time.deltaTime;
            if (_ShowHPBar < 0)
            {
                _isDamaged = false;
            }
        }

    }

    public void EaseMotion(float easeSpeed)
    {
        if (_easeHealthSlider.value != _healthSlider.value)
        {
            _easeHealthSlider.value = Mathf.Lerp(_easeHealthSlider.value, _health, easeSpeed);
        }
    }
}
