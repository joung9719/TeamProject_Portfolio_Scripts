using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInfo : MonoBehaviour
{
    [Header("PlayerInform")]
    public int playerHP; //플레이어 체력
    public int playerAP; //플레이어 기력
    public float playerATK; //플레이어 공격력
    public float playerDEF; //플레이어 방어력
    public float playerSPD; //플레이어 스피드

    public int maxHP;
    public int maxAP;

    [Header("PlayerCurrency")]
    public int playerGold;
    public int playerScore;

    public CharacterStat stat;

    public bool initialized { get; private set; }

    [HideInInspector]
    bool playerDie; //사망 상태 플래그
    void Awake()
    {
        playerHP = 100;
        playerAP = 0;
        playerATK = 0f;
        playerDEF = 0f;
        playerSPD = 0f;

        playerGold = 0;
        playerScore = 0;

        maxHP = 100;
        maxAP = 5;
        initialized = false;
    }

    public void ApplyServer(int hp, int ap, float atk, float def, float spd)
    {
        maxHP = (hp>0)?hp:(maxHP>0?maxHP:100);
        maxAP = (ap>0)?ap:(maxAP>0?maxAP:100);
        
        playerHP = hp;
        playerAP = ap;
        
        playerATK = atk;
        playerDEF = def;
        playerSPD = spd;
        
        initialized = true;
        // 필요해지면 여기서 HUD 갱신 이벤트 호출 추가
    }

    public void ResetBasic()
    {
        playerDie = false;
    }

    public void ApplyStat()
    {
        playerHP  = Mathf.Max(0, stat.baseHP + stat.plusHP - stat.minusHP);
        playerAP  = Mathf.Max(0, stat.baseAP + stat.plusAP - stat.minusAP);
        playerATK = Mathf.Max(0, stat.baseATK + stat.plusATK - stat.minusATK);
        playerDEF = Mathf.Max(0, stat.baseDEF + stat.plusDEF - stat.minusDEF);
        playerSPD = Mathf.Max(0.1f, stat.baseSPD + stat.plusSPD - stat.minusSPD);
    }
}