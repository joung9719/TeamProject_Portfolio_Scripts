using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Windows.Speech;

public class EnemyCtrl : MonoBehaviour, IDamageable
{
    public Animator EnemyAnim;                 // 적 애니메이션
    public Transform EnemyTarget;             // 타겟(플레이어)
    public NavMeshAgent EnemyAgent;           // NavMeshAgent
    ItemDropSpawner_Enemy _dropper;

    [Header("공통 설정")]
    public int EnemyHp = 100;
    public int BossHP = 1000;
    public int NamedBosshp = 500;
    public float EnemySpeed;
    public float EnemyTraceDist;
    public float EnemyAttackDist;
    public float EnemyFindPlayerDist;
    public bool enemyAttacking = false;

    [Header("HUD용Max수치 - 마건우 추가")]
    public int bossMaxHP = 1000;
    public int namedBossMaxHP = 500;

    [Header("보스 종류 체크")]
    public bool isBoss = false;
    public bool isEnemy = false;
    public bool isNamedBoss = false;
    public bool EnemyIsDie = false;

    [Header("이팩트")]
    public GameObject enemyBloodEffect;
    [Header("사운드 - 공통")]
    public AudioClip hitSfx;            // 피격 공통


    [Header("사운드 - 일반몹")]
    public AudioClip enemyAttackSfx;    // 일반몹 공격 1종
    public AudioClip enemydieSfx;       //일반몹 죽는 소리
    public AudioClip enemywalkSfx;      //일반몹 걷는소리
    [Header("사운드 - 중간보스 패턴")]  // Named Boss (배기/찌르기/발찍기/연속)
    public AudioClip namedSlashSfx;         // 패턴 0 : 배기 공격
    public AudioClip namedDoubleThrustSfx;  // 패턴 1 : 두 번 찌르기
    public AudioClip namedStompSfx;         // 패턴 2 : 발찍기
    public AudioClip namedComboSfx;         // 패턴 3 : 연속공격
    public AudioClip namedDieSfx;      //중간보스 죽는 소리
    public AudioClip namedWalkSfx;     //중간보스 걷는소리
    [Header("사운드 - 보스 패턴")]
    public AudioClip bossSmashSfx;      // 패턴 0 : 손 내려찍기(Attack)
    public AudioClip bossClawSfx;       // 패턴 1 : 손 할퀴기(HandAttack)
    public AudioClip bossBreathSfx;     // 브레스 발동/지속
    public AudioClip bossDieSfx;        //보스 죽는소리

    public AudioClip bossWalkSfx;     //보스 걷는소리

    [Header("중간보스")]
    public bool namedBossPattern = false;
    public bool namedBossdelay;
    private int namedBossPatternLast = -1;
    private bool rootMotion;

    [Header("보스 필살기")]
    public bool SkillUse = false;
    public bool BreathUse = false;
    public bool BossFly = false;
    public bool TailAttack = false;
    public bool BreathPoinMove = false;
    public bool BossStartPoint = false;
    public Transform breathPoint;
    public float breathPointDist = 0.5f;

    PhotonConductor _conductor;               // ★ 웨이브/보스 사망 보고용

    [Header("보스 스킬 UI")]
    [SerializeField] GameObject skillUI;
    [SerializeField] TextMeshProUGUI skillText;
    [SerializeField, TextArea(2, 4)]
    string skillMessage = "보스가 강력한 브레스를 시작합니다.\n구조물 뒤로 이동하세요";
    [SerializeField] float skillMessageTime = 3f;
    [SerializeField] bool skillFadeEffecr = true;
    [SerializeField] float skillFadeSpeed = 2f;

    CanvasGroup _skillCanvas;
    Coroutine _skillCo;
    bool _skillHitActive;

    void Awake()
    {
        EnemyAnim = GetComponentInChildren<Animator>();
        if (EnemyAnim != null)
        {
            EnemyAnim.applyRootMotion = false;
        }

        EnemyAgent = GetComponent<NavMeshAgent>();

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            EnemyTarget = playerObj.transform;
        }
        else
        {
            EnemyTarget = null;
        }

        if (EnemyAgent != null)
            EnemyAgent.updateRotation = false;

        _dropper = FindObjectOfType<ItemDropSpawner_Enemy>();
        enemyAttacking = false;

        if (skillUI != null)
        {
            skillUI.SetActive(false);
            if (skillFadeEffecr)
            {
                _skillCanvas = skillUI.GetComponent<CanvasGroup>();
                if (_skillCanvas == null)
                    _skillCanvas = skillUI.AddComponent<CanvasGroup>();
                _skillCanvas.alpha = 0f;
            }
        }


        if (PhotonNetwork.isMasterClient)
        {
            _conductor = FindObjectOfType<PhotonConductor>();
        }
    }

    void Start()
    {
        if (isBoss)
        {
            var breathObj = GameObject.FindGameObjectWithTag("BreathPoint");
            if (breathObj != null)
            {
                breathPoint = breathObj.transform;
            }
        }

        if (isNamedBoss)
        {
            StartCoroutine(NamedBossPatternLoop());
        }
    }

    void Update()
    {
        // 이미 죽었으면 아무 것도 안 함
        if (EnemyIsDie)
            return;


        if (EnemyTarget == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                EnemyTarget = playerObj.transform;
            }
            else
            {
                // 아직 플레이어가 없으면 이번 프레임 종료
                return;
            }
        }

        // 일반 몬스터
        if (isEnemy && !isBoss && !isNamedBoss)
        {
            if (!enemyAttacking && !SkillUse && !BreathUse)
            {
                EnemyMoveTarget();
            }
        }

        // 보스 이동 + 브레스 중 회전
        if (isBoss)
        {
            if (!enemyAttacking && !SkillUse && !BreathUse)
            {
                EnemyMoveTarget();
            }

            if (BreathUse)
            {
                Vector3 lookPlayer = EnemyTarget.position - transform.position;
                lookPlayer.y = 0;
                Quaternion breathQuaterion = Quaternion.Euler(0, 180f, 0);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, breathQuaterion, Time.deltaTime * 3f);
            }
        }

        // 보스 플라이 → 브레스 포인트로 이동
        if (isBoss && BossFly && !BreathUse && !BreathPoinMove && breathPoint != null)
        {
            if (!PhotonNetwork.connected || PhotonNetwork.isMasterClient)
            {
                if (EnemyAgent != null)
                {
                    if (EnemyAgent.isStopped)
                        EnemyAgent.isStopped = false;

                    EnemyAgent.SetDestination(breathPoint.position);

                    float distToBreath =
                        Vector3.Distance(transform.position, breathPoint.position);
                    if (distToBreath <= breathPointDist)
                    {
                        EnemyAgent.isStopped = true;
                        BossFly = false;
                        BossBreath();
                    }
                }
            }
        }

        // 중간보스는 항상 플레이어를 바라보게
        if (isNamedBoss)
        {
            LookPlayer();
        }

        // 공격/브레스 중이 아닐 때는 일반적으로 플레이어 바라봄
        if (!enemyAttacking && !BreathUse)
        {
            LookPlayer();
        }

        if (isEnemy)
        {
            EnemyDie();
        }
        if (isBoss)
        {
            BossDie();
        }
        if (isNamedBoss)
        {
            NamedBossDie();
        }
    }

    ////////////////// 보스 스킬 //////////////////

    public void BossSkillStart()
    {
        if (!isBoss)
            return;

        if (BreathPoinMove)
            return;

        if (BossHP > 500)
            return;
        if (SkillUse)
            return;

        SkillUse = true;
        EnemyAgent.isStopped = true;
        BossSkillHint();

        var net = GetComponent<EnemyNetworkSync>();
        if (net != null)
        {
            net.SendTrigger(EnemyNetworkSync.AnimEvt.BreathStart);
        }
        else
        {
            EnemyAnim.SetTrigger("BreathStart");
        }

    }

    public void BossFlyStart()
    {
        BossFly = true;

        if (!PhotonNetwork.connected || PhotonNetwork.isMasterClient)
        {
            if (EnemyAgent != null)
            {
                EnemyAgent.isStopped = false;
                if (breathPoint != null)
                {
                    EnemyAgent.SetDestination(breathPoint.position);
                }
            }
        }

        var net = GetComponent<EnemyNetworkSync>();
        if (net != null)
        {
            net.SendTrigger(EnemyNetworkSync.AnimEvt.FlyStart);
        }
        else if (EnemyAnim != null)
        {
            EnemyAnim.SetTrigger("FlyStart");
        }
    }

    public void BossFlyEnd()
    {
        BossFly = false;
        EnemyAgent.isStopped = true;

        var net = GetComponent<EnemyNetworkSync>();
        if (net != null)
        {
            net.SendTrigger(EnemyNetworkSync.AnimEvt.FlyEnd);
        }
        else
        {
            EnemyAnim.SetTrigger("FlyEnd");
        }
    }

    public void BossBreath()
    {
        // 브레스는 보스 생애 한 번만
        if (BreathPoinMove)
            return;

        BreathUse = true;
        BreathPoinMove = true;

        EnemyAgent.isStopped = true;
        if (breathPoint != null)
        {
            EnemyAgent.SetDestination(breathPoint.position);
        }

        var net = GetComponent<EnemyNetworkSync>();
        if (net != null)
        {
            net.SendTrigger(EnemyNetworkSync.AnimEvt.Breath);
        }
        else if (EnemyAnim != null)
        {
            EnemyAnim.SetTrigger("Breath");
        }
        if (bossBreathSfx != null && scSoundManager.Instance != null)
        {
            scSoundManager.Instance.PlaySFX(transform.position, bossBreathSfx);
        }

        // 실제 브레스 공격 시작(콜라이더 ON) – 마스터만
        if (!PhotonNetwork.connected || PhotonNetwork.isMasterClient)
        {
            var breath = GetComponentInChildren<BreathAttack>();
            if (breath != null)
            {
                breath.StartBreath();
            }
        }
    }

    public void BossBreathEnd()
    {
        BreathUse = false;
        if (EnemyAgent != null)
            EnemyAgent.isStopped = true;

        // 브레스 종료(콜라이더 OFF) – 마스터만
        if (!PhotonNetwork.connected || PhotonNetwork.isMasterClient)
        {
            var breath = GetComponentInChildren<BreathAttack>();
            if (breath != null)
            {
                breath.EndBreath();
            }
        }

        var net = GetComponent<EnemyNetworkSync>();
        if (net != null)
        {
            net.SendTrigger(EnemyNetworkSync.AnimEvt.BreathEnd);
        }
        else
        {
            EnemyAnim.SetTrigger("BreathEnd");
        }
    }

    public void LoopFlyStart()
    {
        BossFly = true;
        EnemyAgent.isStopped = true;

        var net = GetComponent<EnemyNetworkSync>();
        if (net != null)
        {
            net.SendTrigger(EnemyNetworkSync.AnimEvt.FlyStart);
        }
        else
        {
            EnemyAnim.SetTrigger("FlyStart");
        }
    }

    public void LoopFlyEnd()
    {
        BossFly = false;
        EnemyAgent.isStopped = true;

        var net = GetComponent<EnemyNetworkSync>();
        if (net != null)
        {
            net.SendTrigger(EnemyNetworkSync.AnimEvt.FlyEnd);
        }
        else
        {
            EnemyAnim.SetTrigger("FlyEnd");
        }
    }

    public void BossSkillEnd()
    {
        SkillUse = false;
        EnemyAgent.isStopped = false;
        EnemyAnim.SetTrigger("Move");
    }

    public void BossTailAttack()
    {
        if (!isBoss)
            return;
        if (BossHP > 500)
            return;
        if (enemyAttacking || SkillUse || BreathUse)
            return;

        enemyAttacking = true;
        EnemyAgent.isStopped = true;

        var net = GetComponent<EnemyNetworkSync>();
        if (net != null)
        {
            net.SendTrigger(EnemyNetworkSync.AnimEvt.TailAttack);
        }
        else
        {
            EnemyAnim.SetTrigger("TailAttack");
        }

        StartCoroutine(AttackCoolTime(1.5f));
    }

    public void BossSkillHint()
    {
        if (skillUI == null || skillText == null)
            return;
        if (_skillHitActive)
            return;

        _skillHitActive = true;

        if (string.IsNullOrEmpty(skillMessage))
        {
            skillMessage = "보스가 강력한 브레스를 시작합니다.\n구조물 뒤로 이동하세요";
        }

        skillText.text = skillMessage;
        skillUI.SetActive(true);

        if (skillFadeEffecr && _skillCanvas != null)
        {
            if (_skillCo != null)
                StopCoroutine(_skillCo);
            _skillCo = StartCoroutine(SkillHintFade());
        }
        else
        {
            if (_skillCo != null)
                StopCoroutine(_skillCo);
            _skillCo = StartCoroutine(SkillHintSimple());
        }
    }

    IEnumerator SkillHintSimple()
    {
        float t = (skillMessageTime > 0f) ? skillMessageTime : 3f;
        yield return new WaitForSeconds(t);
        skillUI.SetActive(false);
        _skillHitActive = false;
    }

    IEnumerator SkillHintFade()
    {
        _skillCanvas.alpha = 0f;
        while (_skillCanvas.alpha < 1f)
        {
            _skillCanvas.alpha += Time.deltaTime * skillFadeSpeed;
            yield return null;
        }
        _skillCanvas.alpha = 1f;

        float t = (skillMessageTime > 0f) ? skillMessageTime : 3f;
        yield return new WaitForSeconds(t);

        while (_skillCanvas.alpha > 0f)
        {
            _skillCanvas.alpha -= Time.deltaTime * skillFadeSpeed;
            yield return null;
        }
        _skillCanvas.alpha = 0f;

        skillUI.SetActive(false);
        _skillHitActive = false;
    }

    public bool BossSkillAnimEnd(string AnimName)
    {
        return EnemyAnim.GetCurrentAnimatorStateInfo(0).IsName(AnimName) &&
               EnemyAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;
    }

    ///////////////// 적 행동 //////////////////

    public void EnemyIdle()
    {
        if (EnemyTarget == null)
            return;

        float dist = Vector3.Distance(transform.position, EnemyTarget.position);
        if (dist <= EnemyFindPlayerDist)
        {
            EnemyAgent.isStopped = true;
            EnemyAnim.SetTrigger("Idle");
        }
    }

    public void EnemyMoveTarget()
    {
        if (EnemyTarget == null)
        {
            EnemyAgent.isStopped = true;
            EnemyAnim.SetTrigger("Idle");
            return;
        }

        float dist = Vector3.Distance(transform.position, EnemyTarget.position);

        if (dist <= EnemyAttackDist)
        {
            if (isBoss)
            {
                BossNormalAttack();
            }
            else
            {
                EnemyAttack();
            }
            return;
        }

        if (dist <= EnemyTraceDist)
        {
            EnemyAgent.isStopped = false;
            if (EnemySpeed > 0f)
                EnemyAgent.speed = EnemySpeed;

            EnemyAgent.SetDestination(EnemyTarget.position);

            if (!enemyAttacking && !SkillUse && !BreathUse)
            {
                EnemyAnim.SetTrigger("Move");
            }

            LookPlayer();
            return;
        }

        EnemyAgent.isStopped = true;
        EnemyAnim.SetTrigger("Idle");
    }

    public void EnemyAttack()
    {
        if (enemyAttacking || EnemyTarget == null || SkillUse || BreathUse)
            return;

        float dist = Vector3.Distance(transform.position, EnemyTarget.position);
        if (dist > EnemyAttackDist)
            return;

        enemyAttacking = true;
        EnemyAgent.isStopped = true;
        EnemyAnim.ResetTrigger("Move");
        EnemyAnim.SetTrigger("Attack");
        if (enemyAttackSfx != null && scSoundManager.Instance != null)
        {
            scSoundManager.Instance.PlaySFX(transform.position, enemyAttackSfx);
        }
        StartCoroutine(AttackCoolTime(1.5f));
    }

    public void BossNormalAttack()
    {
        if (!isBoss) return;
        if (enemyAttacking || EnemyTarget == null || SkillUse || BreathUse)
            return;

        float dist = Vector3.Distance(transform.position, EnemyTarget.position);
        if (dist > EnemyAttackDist)
            return;
        if (BossHP <= 500 && !BreathPoinMove)
        {
            BossSkillStart();
            return;
        }

        enemyAttacking = true;
        EnemyAgent.isStopped = true;

        EnemyAnim.ResetTrigger("Move");

        int pattern = Random.Range(0, 2);
        if (pattern == 0)
        {
            EnemyAnim.SetTrigger("Attack");
            if (bossClawSfx != null && scSoundManager.Instance != null)
            {
                scSoundManager.Instance.PlaySFX(transform.position, bossClawSfx);
            }
        }
        else
        {
            EnemyAnim.SetTrigger("HandAttack");
        }
        if (bossSmashSfx != null && scSoundManager.Instance != null)
        {
            scSoundManager.Instance.PlaySFX(transform.position, bossSmashSfx);
        }

        StartCoroutine(AttackCoolTime(1.5f));
    }

    IEnumerator AttackCoolTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        enemyAttacking = false;
        EnemyAgent.isStopped = false;
        EnemyAnim.ResetTrigger("Attack");
        EnemyAnim.ResetTrigger("HandAttack");
    }


    public IEnumerator NamedBossPatternLoop()
    {
        // 네트워크이면 마스터만 패턴 처리
        if (PhotonNetwork.connected && !PhotonNetwork.isMasterClient)
            yield break;

        while (!EnemyIsDie)
        {
            // 타겟 없으면 매 프레임 다시 찾기
            if (EnemyTarget == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    EnemyTarget = playerObj.transform;
                }
                else
                {
                    yield return null;
                    continue;
                }
            }

            if (!enemyAttacking && !SkillUse && !BreathUse)
            {
                enemyAttacking = true;
                EnemyAgent.isStopped = true;

                float dist = Vector3.Distance(transform.position, EnemyTarget.position);
                int pattern;

                if (dist > EnemyTraceDist)
                {
                    pattern = 3;
                }

                do
                {
                    pattern = Random.Range(0, 4);
                }
                while (pattern == namedBossPatternLast);

                namedBossPatternLast = pattern;

                switch (pattern)
                {
                    case 0:
                        EnemyAgent.isStopped = true;
                        EnemyAnim.SetTrigger("Attack");
                        if (namedSlashSfx != null && scSoundManager.Instance != null)
                        {
                            scSoundManager.Instance.PlaySFX(transform.position, namedSlashSfx);
                        }
                        yield return new WaitForSeconds(3f);
                        break;

                    case 1:
                        yield return MovePlayer(EnemyAttackDist);
                        EnemyAnim.SetTrigger("Attack02");
                        if (namedDoubleThrustSfx != null && scSoundManager.Instance != null)
                        {
                            scSoundManager.Instance.PlaySFX(transform.position, namedDoubleThrustSfx);
                        }
                        yield return new WaitForSeconds(3f);
                        break;

                    case 2:
                        yield return MovePlayer(EnemyAttackDist);
                        EnemyAnim.SetTrigger("Attack03");
                        if (namedStompSfx != null && scSoundManager.Instance != null)
                        {
                            scSoundManager.Instance.PlaySFX(transform.position, namedStompSfx);
                        }
                        yield return new WaitForSeconds(5f);
                        break;

                    case 3:
                        yield return MovePlayer(EnemyAttackDist);
                        EnemyAnim.SetTrigger("Attack04");
                        if (namedComboSfx != null && scSoundManager.Instance != null)
                        {
                            scSoundManager.Instance.PlaySFX(transform.position, namedComboSfx);
                        }
                        yield return new WaitForSeconds(4f);
                        break;
                }

                EnemyAnim.ResetTrigger("Attack");
                EnemyAnim.ResetTrigger("Attack02");
                EnemyAnim.ResetTrigger("Attack03");
                EnemyAnim.ResetTrigger("Attack04");

                yield return new WaitForSeconds(1.5f);
                enemyAttacking = false;
                EnemyAgent.isStopped = false;
            }

            yield return null;
        }
    }

    IEnumerator MovePlayer(float Stop)
    {
        EnemyAgent.isStopped = false;
        EnemyAgent.speed = EnemySpeed;

        while (EnemyTarget != null &&
               Vector3.Distance(transform.position, EnemyTarget.position) > Stop)
        {
            EnemyAgent.isStopped = false;
            EnemyAgent.SetDestination(EnemyTarget.position);
            LookPlayer();
            yield return null;
        }

        EnemyAgent.isStopped = true;
        yield return new WaitForSeconds(0.1f);
    }

    public void EnemyDie()
    {
        if (EnemyIsDie) return;

        if (EnemyHp <= 0)
        {
            EnemyAgent.isStopped = true;
            EnemyIsDie = true;
            EnemyAnim.SetTrigger("Die");
            if (enemydieSfx != null && scSoundManager.Instance != null)
            {
                scSoundManager.Instance.PlaySFX(transform.position, enemydieSfx);
            }

            if (_dropper != null)
            {
                _dropper.TryDropOnDeath(transform.position);
            }

            if (PhotonNetwork.isMasterClient)
            {
                var sync = GetComponent<EnemyNetworkSync>();
                if (sync != null) sync.SendDie();

                if (_conductor != null)
                {
                    _conductor.OnEnemyDied(transform.position);
                }
            }
        }
    }

    public void BossDie()
    {
        if (BossHP <= 0 && !EnemyIsDie)
        {
            EnemyAgent.isStopped = true;
            EnemyIsDie = true;
            EnemyAnim.SetTrigger("BossDie");

            if (bossDieSfx != null & scSoundManager.Instance != null)
            {
                scSoundManager.Instance.PlaySFX(transform.position, bossDieSfx);
            }

            if (PhotonNetwork.isMasterClient)
            {
                var sync = GetComponent<EnemyNetworkSync>();
                if (sync != null) sync.SendDie();

                if (_conductor != null)
                {
                    _conductor.OnEnemyDied();
                }

                var stage = FindObjectOfType<PhotonStageManager>();
                if (stage != null)
                {
                    stage.ReportBossDead();
                }
            }

            Destroy(gameObject, 8.0f);
        }
    }

    public void NamedBossDie()
    {
        if (NamedBosshp <= 0 && !EnemyIsDie)
        {
            EnemyAgent.isStopped = true;
            EnemyIsDie = true;
            EnemyAnim.SetTrigger("Die");
            if (namedDieSfx != null && scSoundManager.Instance != null)
            {
                scSoundManager.Instance.PlaySFX(transform.position, namedDieSfx);
            }

            if (PhotonNetwork.isMasterClient)
            {
                var sync = GetComponent<EnemyNetworkSync>();
                if (sync != null) sync.SendDie();

                if (_conductor != null)
                {
                    _conductor.OnEnemyDied(transform.position);
                }
            }

            Destroy(gameObject, 8.0f);
        }
    }

    public void LookPlayer()
    {
        if (EnemyTarget == null)
            return;

        Vector3 dir = EnemyTarget.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation =
                Quaternion.Slerp(transform.rotation, lookRot, 360f * Time.deltaTime);
        }
    }

    public void ApplyDamage(int dameg, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isBoss)
        {
            BossHP -= (int)dameg;
            BossDie();
        }
        if (isEnemy)
        {
            EnemyHp -= (int)dameg;
            if (PhotonNetwork.isMasterClient)
            {
                var sync = GetComponent<EnemyNetworkSync>();
                if (sync != null) sync.SendHp(EnemyHp);
            }
            EnemyDie();
        }
        if (isNamedBoss)
        {
            NamedBosshp -= (int)dameg;
            NamedBossDie();
        }

        if (enemyBloodEffect != null)
        {
            Instantiate(enemyBloodEffect, hitPoint, Quaternion.LookRotation(hitNormal));
        }
    }
    public void EnemyWalkSfx()
    {
        if (enemywalkSfx != null && scSoundManager.Instance != null)
        {
            scSoundManager.Instance.PlaySFX(transform.position, enemywalkSfx);
        }
    }

    public void NamedWalkSfx()
    {
        if (namedWalkSfx != null && scSoundManager.Instance != null)
        {
            scSoundManager.Instance.PlaySFX(transform.position, namedWalkSfx);
        }
    }

    public void BossWalkSfx()
    {
        if (bossWalkSfx != null && scSoundManager.Instance != null)
        {
            scSoundManager.Instance.PlaySFX(transform.position, bossWalkSfx);
        }
    }
}
