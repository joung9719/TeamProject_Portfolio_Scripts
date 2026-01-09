using System.Collections;
using UnityEngine;

public class BossTailAttack : MonoBehaviour
{
    [SerializeField] int TailAttackDamge = 25;//꼬리 공격 대미지
    [SerializeField] Collider tailCollider;
    public bool CoolTime = false;//쿨다임(안하면 계속 꼬리 공격함)
    // Start is called before the first frame update
    void Awake()
    {
        tailCollider.enabled = false;
    }
    public void StartTailAttack()
    {
        if (CoolTime)
        {
            return;
        }
        if (PhotonNetwork.isMasterClient)
        {
            tailCollider.enabled = true;
        }
    }
    public void EndTailAttack()
    {
        if (PhotonNetwork.isMasterClient)
        {
            tailCollider.enabled = false;
        }
        StartCoroutine(AttackCoolTime(3f));
    }

    private IEnumerator AttackCoolTime(float delay)
    {
        CoolTime = true;
        yield return new WaitForSeconds(delay);
        CoolTime = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.isMasterClient)
            return;
        if (other.CompareTag("Player"))
        {
            IDamageable player = other.GetComponent<IDamageable>();
            if (player != null)
            {
                player.ApplyDamage(TailAttackDamge, other.transform.position, Vector3.up);
                Debug.Log("아야");
            }
        }
    }
}
