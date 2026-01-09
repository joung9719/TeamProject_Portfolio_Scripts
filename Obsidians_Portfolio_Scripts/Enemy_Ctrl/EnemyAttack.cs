using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] int AttackDamge = 10;
    [SerializeField] Collider AttackCollider;
    EnemyCtrl enemyCtrl;


    void Awake()
    {
        if (AttackCollider == null)
        {
            AttackCollider = GetComponent<Collider>();
        }
        AttackCollider.enabled = false;
        enemyCtrl = GetComponent<EnemyCtrl>();
    }

    public void StratAttack()
    {
        if (PhotonNetwork.isMasterClient)
        {
            AttackCollider.enabled = true;
        }
    }

    public void EndAttack()
    {
        AttackCollider.enabled = false;
        if (enemyCtrl.isNamedBoss)
        {

        }
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
                player.ApplyDamage(AttackDamge, other.transform.position, Vector3.up);
                Debug.Log("아야");
            }
        }
    }
}
