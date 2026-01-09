using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif
[RequireComponent(typeof(CharacterController))]
public class PlayerCtrl : MonoBehaviour, IDamageable
{
    [Header("플레이서 스텟 및 정보")]
    private int Player_Life;//플레이어 체력 -> PlayerState.cs?
    public GameObject Damage_Effect;//데미지 효과 프리펩 -> PlayerHUD.cs?
    public GameObject muzzleFlashs;//플레시 효과
    private bool isFlashing;
    public float flashDuration = 0.1f;
    public Projector Deamge_Projector;//데미지 프로젝터 연결 -> PlayerHUD.cs?

    public float Move_Speed = 10f;//플레이어 이동 속도
    public float Player_RunSpeed = 20f;//플레이어 달리기 속도
    public float Player_Turn = 5f;//플레이어 회전 속도
    public float Player_Gravity = 9.8f;//플레이어 중력
    AudioClip playerVoiceAudio = null; //플레이어 음성 소스
    PlayerAnim playerAnim;//플레이어 에니메이션을 참조로 받아오기위한거
    PlayerShotPistol playerShotPistol;//플레이어가 권총을 쏠 수 있게 참조로 받아오는거
    PlayerShotRifle playerShotRifle;//플레이어가 소총을 쏠 수 있게 참조
    PlayerInfo playerInfo;//플레이어 체력 받아오기
    [Header("플레이어 이동")]
    private CharacterController player;//캐릭터 콜라이더를 이용하여 플레이어 이동
    private Vector3 moveDir; //플레이어 이동
    public GameObject cam;//메인 카메라 이동방향으로 움직이게 할려고 넣음
    private Camera followCamera;//시작시 카메라 바로 인스펙터에 넣기
    private float verticalVelocity = 0f; //플레이어에게 중력 적용
    MuzzleFlash muzzleFlash;
  [Header("카메라")]
    public float xCamSpeed;//카메라 X축 이동 속도
    public float yCamSpeed;//카메라 Y축 이동 속도

    private float xRotation;//카메라 X축 각도?
    private float yRotation;//카메라 Y축 각도?
    private Transform playerTransfrom;//플레이어 포지션
    public Transform camTransfrom;//카메라 포지션
   [Header("플레이어 사운드")]
    public AudioClip pistolShotSfx;
    public AudioClip rifleShotSfx;
    public AudioClip walkStepSfx;
    public AudioClip runSfx;
    public AudioClip dieSfx;
    public AudioClip hitSfx;
    public AudioClip jumpSfx;

    void Awake()
    {
        player = GetComponent<CharacterController>();
        playerVoiceAudio = GetComponent<AudioClip>();
        playerAnim = GetComponent<PlayerAnim>();
        playerShotPistol = GetComponent<PlayerShotPistol>();
        playerShotRifle = GetComponent<PlayerShotRifle>();
        playerInfo = GetComponent<PlayerInfo>();
        muzzleFlash=GetComponent<MuzzleFlash>();
        playerTransfrom = GetComponent<Transform>();
        cam = GetComponent<GameObject>();
    }
    void Start()
    {
        ///마우스 커서 숨기기///
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //카메라 정보를 받아 태그로 카메라 가져오기(캐릭터는 카메라 방향으로 움직이게 할려고)
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        ///카메라 집어넣기////
        if (camTransfrom == null)
        {
            camTransfrom = Camera.main.transform;
        }
     

    }

    void Update()
    {

        //입력 값 처리(좌우 이동)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputMove = new Vector3(h, 0f, v).normalized;

        //카메라 방향으로 이동
        Vector3 camF = cam.transform.forward;//앞 뒤
        Vector3 camR = cam.transform.right;//좌 우
        camF.y = 0f;
        camR.y = 0f;
        camF.Normalize();
        camR.Normalize();

        moveDir = (camF * v + camR * h).normalized;
        bool isMoveing = moveDir.sqrMagnitude > 0.01f;

        //플레이어 점프
        if (Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = 8f;//중력
            playerAnim.SetJump(true);

            GetComponent<PlayerNetworkSync>().SendBool("Jump", true);
        }
        else
        {
            playerAnim.SetJump(false);

            GetComponent<PlayerNetworkSync>().SendBool("Jump", false);
        }
        //플레이어 이동 및 회전 
        playerAnim.SetMove(isMoveing);
        if (isMoveing)
        {
            Quaternion rot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Player_Turn * Time.deltaTime);
        }
        //플레이어 사망
        if (playerInfo.playerHP <= 0.0f)
        {
           if(dieSfx!=null&&scSoundManager.Instance!=null)
            {
                scSoundManager.Instance.PlaySFX(transform.position,dieSfx);
            }
            if (PhotonView.Get(this).isMine)
            {
                var table = new ExitGames.Client.Photon.Hashtable();
                table["DEAD"] = true;
                PhotonNetwork.player.SetCustomProperties(table);
            }
            GetComponent<PlayerCtrl>().enabled = false;
            player.enabled = false;
            playerAnim.SetDie(true);
            Destroy(gameObject, 2.0f);
        }

        //플레이어가 총을 쏜다
        if (Input.GetMouseButton(0))
        {
            //플레이어가 마우스버튼을 눌르면 총을 쏜다 
            playerShotPistol?.StartFire();
            playerShotRifle?.StartFire();
            muzzleFlash.OnMouseDown();
        }
        else
        {
            //마우스를 때면 총쏘기 정지 
            playerShotPistol?.StopFire();
            playerShotRifle?.StopFire();
        }
        //플레이어 특수 능력
        if(Input.GetKeyDown(KeyCode.Q))
        {
            playerAnim.SetThrow(true);
        }
        else
        {
            playerAnim.SetThrow(false);
        }
        ////플레이어 상호작용
        if(Input.GetKeyDown(KeyCode.F))
        {
            
        }
        else
        {
            
        }
    }
   

    void FixedUpdate()
    {
        //플레이어 달리기
        bool running = Input.GetKey(KeyCode.LeftShift);
        if (running)
        {
            playerAnim.SetRun(true);
        }
        else
        {
            playerAnim.SetRun(false);
        }
        //플레이어에게 중력 적용
        if (player.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity -= Player_Gravity * Time.deltaTime;
        }
        //플레이어이동,달리기,중력을 최종적으로 적용?
        Vector3 fianlMove = moveDir * (running ? Player_RunSpeed : Move_Speed);
        fianlMove.y = verticalVelocity;
        player.Move(fianlMove * Time.deltaTime);
    }

    void LateUpdate()
    {
        //////카메라 해결/////////
        float camTime = Time.deltaTime;
        float xcam = Input.GetAxisRaw("Mouse X") * xCamSpeed * camTime;
        float ycam = Input.GetAxisRaw("Mouse Y") * yCamSpeed * camTime;
        ///카메라의 Y축을 플레이어트렌스 폼의 각도를 돌린다(위,아레)
        yRotation += xcam;
        playerTransfrom.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        //카메라의 X축을 카메라 트렌스폼의 각도를 (좌,우)돌린다
        xRotation -= ycam;
        xRotation = Mathf.Clamp(xRotation, -15f, 15f);
        camTransfrom.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    public void ApplyDamage(int dameg, Vector3 hitPoint, Vector3 hitNornal)
    {
        playerInfo.playerHP -= (int)dameg;
        if (playerInfo.playerHP <= 0)
        {
            playerAnim.SetDie(true);
        }
        if(hitSfx!=null&&scSoundManager.Instance!=null)
        {
            scSoundManager.Instance.PlaySFX(transform.position,hitSfx);
        }
        if (Damage_Effect != null)
        {
            Instantiate(Damage_Effect, hitPoint, Quaternion.LookRotation(hitNornal));
        }
    }

    public void PlayerPistolSfx()
    {
        if(pistolShotSfx!=null&&scSoundManager.Instance!=null)
        {
            scSoundManager.Instance.PlaySFX(transform.position,pistolShotSfx);
        }
    }

    public void PlayerRifleSfx()
    {
        if(rifleShotSfx!=null&&scSoundManager.Instance!=null)
        {
            scSoundManager.Instance.PlaySFX(transform.position,rifleShotSfx);
        }
    }

    public void PlayerWalkSfx()
    {
        if(walkStepSfx!=null&&scSoundManager.Instance!=null)
        {
            scSoundManager.Instance.PlaySFX(transform.position,walkStepSfx);
        }
    }

    public void PlayerRunSfx()
    {
        if(runSfx!=null&&scSoundManager.Instance!=null)
        {
            scSoundManager.Instance.PlaySFX(transform.position,runSfx);
        }
    }

    public void PlayerJumpSfx()
    {
        if(jumpSfx!=null&&scSoundManager.Instance!=null)
        {
            scSoundManager.Instance.PlaySFX(transform.position,jumpSfx);
        }
    }
}
