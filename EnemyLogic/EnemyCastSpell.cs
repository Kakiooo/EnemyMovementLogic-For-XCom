using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static EnemyMovement;

public class EnemyCastSpell : MonoBehaviour
{
    float _timer, _maxValue,attackTimer;
    public EnemySpell currentSpell;
    PlayerHealth _p_HealthSC;
    Slider _subSpellSlider;
    Canvas _subSpellUI;
    CanvasGroup _spellCanvasGroup;
    Animator _camera_Animator;
    Quaternion _defultRotation;
    EnemyMovement _enemyMove;
    Image _subSpellDis;
    bool _isGetSpell, isInRange;
    public bool _isCastedInTutorial;
    public bool _isAttackEnd;
    [SerializeField] EnemySpell[] enemySpell;
    [SerializeField] float attackWaitingTime;
    EnemySelfStatesControl _refToStateControlf;
    GameObject temPGameOBject;

    private void Awake()
    {

        _timer = 5;
        attackTimer = attackWaitingTime;
        _p_HealthSC = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        _subSpellSlider = transform.GetChild(1).transform.GetChild(0).GetComponent<Slider>();
        _subSpellSlider.value = 0;
        _maxValue = _subSpellSlider.maxValue;

        _spellCanvasGroup = transform.GetChild(1).GetComponent<CanvasGroup>();
        _spellCanvasGroup.alpha = 0;
        _subSpellUI = _spellCanvasGroup.transform.gameObject.transform.GetComponent<Canvas>();
        _defultRotation=_subSpellUI.transform.rotation;

        _camera_Animator = GameObject.Find("GameplayStateDrivenCamera").GetComponent<Animator>();
        _enemyMove=GetComponent<EnemyMovement>();
        

        _subSpellDis = transform.GetChild(1).transform.GetChild(0).transform.GetChild(1).GetComponent<Image>();
        _refToStateControlf = GetComponent<EnemySelfStatesControl>();
    }
    private void Start()
    {
    }

    private void Update()
    {
       
        if (_refToStateControlf.enemyType == "TurretEnemy")
        {
            if (_refToStateControlf.isInRange)//must to use this intherange check to see if player is inside the range BETTER TO REWRITE THIS IN ENEMYCASTSPELL SC
            {
                _refToStateControlf._currentState = EnemySelfStatesControl.enemyState.Attack;
                CastTheSpellAsTurret();
            }
            else
            {
                _timer = 5;
                _spellCanvasGroup.alpha = 0;
                _subSpellSlider.value = 0;
                _subSpellDis.sprite = null;
                _isGetSpell = false;//reset value
                return;
            }
        }
        else
        {
            if (!_refToStateControlf.isInRange && _refToStateControlf._currentState == EnemySelfStatesControl.enemyState.Attack)
            {
                // switch to another state
                print("Yeah its outside of range sryyyyy");
                if (currentSpell != null)
                {
                    _timer = 5;
                    _spellCanvasGroup.alpha = 0;
                    _subSpellSlider.value = 0;
                    _subSpellDis.sprite = null;
                    _isGetSpell = false;//reset value
                }
                _refToStateControlf._currentState = EnemySelfStatesControl.enemyState.Move;//dont change this one
            }
            else if (_refToStateControlf.isInRange && _refToStateControlf._currentState == EnemySelfStatesControl.enemyState.Attack && !_isAttackEnd)
            {
                CastTheSpell();
                //if (temPGameOBject != null)
                //{
                //    temPGameOBject.transform.position = Vector3.MoveTowards(temPGameOBject.transform.position, _p_HealthSC.transform.position, 0.5f*Time.deltaTime);

                //}
            }
        }

        SpellDisplayOnCamera();
    }

    void CastTheSpellAsTurret()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer < 0&& !_isCastedInTutorial)
        {
            if (!_isGetSpell)
            {
                ChooseSpell();
                _isGetSpell = true;
            }
            SpellType_DeterAction();
        }
    }
    void CastTheSpell()
    {
        _timer -= Time.deltaTime;

        if (_timer < 0)
        {
            if (!_isGetSpell)
            {
                ChooseSpell();
                _isGetSpell = true;
            }
            SpellType_DeterAction();

            //DisplaySubSpellWithColliderTrigger(currentSpell.SpellComponents.Count);
        }
    }

    void SpellType_DeterAction()
    {
        if (currentSpell.Spell_Type == "FromSky")
        {
            StartCoroutine(CastViegarTypeSpell_FromSky(currentSpell.SpellComponents.Count)); //timer is too short so the timer enter the countdown again  
        }
        else if (currentSpell.Spell_Type == "FromGround")
        {
            CastProjectileSpell_FromGround(currentSpell.SpellComponents.Count);
        }
    }

    public void ChooseSpell()
    {
        currentSpell = enemySpell[Random.Range(0,enemySpell.Length)];
        print("CurrentSpell" + currentSpell.name);
    }

    IEnumerator CastViegarTypeSpell_FromSky(float spellLength)
    {
        _subSpellDis.sprite = currentSpell.SubSepll;
        _spellCanvasGroup.alpha = 1;
        _subSpellSlider.value += _maxValue * Time.deltaTime / spellLength;
        if (_subSpellSlider.value >= _maxValue && currentSpell.FeedbackPrefab.GetComponent<ParticleSystem>() != null)
        {
            GameObject attackFeedBack = Instantiate(currentSpell.FeedbackPrefab, _p_HealthSC.transform.position, Quaternion.identity);
            Vector3 tempPos = _p_HealthSC.transform.position;
            temPGameOBject = attackFeedBack;
            attackFeedBack.GetComponent<ParticleSystem>().Play();
            float ParticleDuration = attackFeedBack.GetComponent<ParticleSystem>().main.startLifetime.constant;
            _timer = Random.Range(ParticleDuration + 1, ParticleDuration + 3);//reset the value back to defualt
            attackTimer = attackWaitingTime + ParticleDuration;
            _spellCanvasGroup.alpha = 0;
            _subSpellSlider.value = 0;
            _subSpellDis.sprite = null;
            _isGetSpell = false;//reset value
            float animationModifier = 1.5f;

            yield return new WaitForSeconds(ParticleDuration - animationModifier);//wait for some times

            GameObject collider = Instantiate(currentSpell.ColliderObject, tempPos, Quaternion.identity);
            collider.transform.GetComponent<Collider_SC_Enemy>()._SpellDmgToEnemies = currentSpell.Damage;

            if (SceneManager.GetActiveScene().name == "RepositionTutorial") _isCastedInTutorial = true;// if is in the tutorial, enemy will only cast one time spell

            yield return new WaitForSeconds(animationModifier);
            Destroy(attackFeedBack);
            _isAttackEnd = true;
        }
    }

    void CastProjectileSpell_FromGround(float spellLength)
    {
        _subSpellDis.sprite = currentSpell.SubSepll;
        _spellCanvasGroup.alpha = 1;
        _subSpellSlider.value += _maxValue * Time.deltaTime / spellLength;
        if (_subSpellSlider.value >= _maxValue && currentSpell.FeedbackPrefab.GetComponent<ParticleSystem>() == null)
        {
            GameObject attackFeedBack = Instantiate(currentSpell.FeedbackPrefab,transform.position, Quaternion.identity);
            attackFeedBack.GetComponent<Projectile_Spell_Enemy>().GetDamager = _p_HealthSC.transform.position;
            _spellCanvasGroup.alpha = 0;
            _subSpellSlider.value = 0;
            _subSpellDis.sprite = null;
            _isGetSpell = false;//reset value

            if (SceneManager.GetActiveScene().name == "RepositionTutorial") _isCastedInTutorial = true;// if is in the tutorial, enemy will only cast one time spell
            _isAttackEnd = true;
        }
    }

    void SpellDisplayOnCamera()
    {
        //always facing the camera 90 degree rotation along camera movement
        int currentState = _camera_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (currentState == Animator.StringToHash("Start1"))
        {
            _subSpellUI.transform.rotation = _defultRotation;
        }
        else if (currentState == Animator.StringToHash("2"))
        {
            _subSpellUI.transform.rotation = Quaternion.Euler(45, 90, 0);
        }
        else if (currentState == Animator.StringToHash("3"))
        {
            _subSpellUI.transform.rotation = Quaternion.Euler(45, 180, 0);
        }
        else if (currentState == Animator.StringToHash("4"))
        {
            _subSpellUI.transform.rotation = Quaternion.Euler(45, 270, 0);
        }

    }
}
