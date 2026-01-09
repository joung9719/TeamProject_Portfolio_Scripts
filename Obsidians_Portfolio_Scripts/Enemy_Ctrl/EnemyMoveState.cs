using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class EnemyMoveState : EnemyState
{
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_enemyCtrl == null || _enemyCtrl.EnemyTarget == null || _enemyCtrl.EnemyIsDie)
            return;

       
        if (_enemyCtrl.isBoss && !_enemyCtrl.SkillUse && _enemyCtrl.BossHP <= 500f)
        {
           
            if (!PhotonNetwork.connected || PhotonNetwork.isMasterClient)
            {
                _enemyCtrl.BossSkillStart();
            }
           
            return;
        }

        if (_enemyCtrl.isNamedBoss)
            return;

        if (_enemyCtrl.SkillUse || _enemyCtrl.BreathUse || _enemyCtrl.enemyAttacking)
            return;

        float dist = Vector3.Distance(_enemyCtrl.transform.position, _enemyCtrl.EnemyTarget.position);

        if (dist <= _enemyCtrl.EnemyTraceDist)
        {
            _enemyCtrl.EnemyMoveTarget();
        }
        else
        {
            animator.SetTrigger("Idle");
        }
    }
}
