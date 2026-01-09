using System;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerShotPistol : MonoBehaviour
{
    [Header("FireFX")]
    [SerializeField] LineRenderer rayLine; //레이져 발사를 위한 컴포넌트(적을 찾고 총을 발사할때 사용하기위해서)
    [SerializeField] Transform rayDot;     //레이져 도트 타겟을 위한 변수
    [Header("Sound")]
    [SerializeField] AudioSource audioOut;//발사음을 한 곳에서 재생
    public AudioClip fireSfx;
    [Header("Weapons")]
    [SerializeField] int pistoldamage = 10;
    [SerializeField] int range = 50;
    [SerializeField] LayerMask hitMask;
    [SerializeField] Transform firPos;
    bool _wantsToFire;
    PlayerAnim playerAnim;

    void Awake()
    {
        playerAnim = GetComponentInParent<PlayerAnim>();
        //firPos = GetComponent<PlayerShotPistol>().transform;
    }

    void Start()
    {
        //firPos = GameObject.FindGameObjectWithTag("FirePos").GetComponent<Transform>();
        if (rayLine == null)
        {
            rayLine = GetComponent<LineRenderer>();
        }
        hitMask = LayerMask.GetMask("Enemy");
    }
    public void StartFire()
    {
        _wantsToFire = true;//애니메이션 bool넣기
        playerAnim.SetAttack(true);
    }

    public void StopFire()
    {
        _wantsToFire = false;//애니메이션 bool넣기
        playerAnim.SetAttack(false);

        GetComponent<PlayerNetworkSync>()?.SendBool("Shoot", false);
    }

    void Update()
    {

    }

    public void FireOnce()
    {
        if (audioOut && fireSfx) audioOut.PlayOneShot(fireSfx);

        Vector3 origin = firPos.position;
        Vector3 dir = firPos.forward;

        if (Physics.Raycast(origin, dir, out var hit, range, hitMask))
        {
            if (rayLine)
            {
                rayLine.SetPosition(0, origin);
                rayLine.SetPosition(1, hit.point);
                rayLine.enabled = true;
                //Invoke(nameof(HideLine),0.03f);
            }

            if (rayDot)
            {
                rayDot.position = hit.point;
            }

            if (hit.collider.GetComponentInParent<IDamageable>() is IDamageable dmg)
            {
                dmg.ApplyDamage(pistoldamage, hit.point, hit.normal);

            }
            else
            {
                rayLine.enabled = true;
                rayLine.SetPosition(0, origin);
                rayLine.SetPosition(1, origin + dir * range);
            }
        }

        GetComponent<PlayerNetworkSync>()?.SendBool("Shoot", true);
    }
    void HideLine()
    {
        if (rayLine) rayLine.enabled = false;
    }

    void OnDisable()
    {
        GetComponent<PlayerNetworkSync>()?.SendBool("Shoot", false);
    }
}