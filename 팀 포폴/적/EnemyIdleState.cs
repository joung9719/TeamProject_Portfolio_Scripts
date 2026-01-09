using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       base.OnStateEnter(animator,stateInfo,layerIndex);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_enemyCtrl == null || _enemyCtrl.EnemyTarget == null || _enemyCtrl.EnemyIsDie)
            return;
            if(_enemyCtrl.isNamedBoss)
            return;
            if(_enemyCtrl.SkillUse||_enemyCtrl.BreathUse||_enemyCtrl.enemyAttacking)
            return;

        float dist = Vector3.Distance(_enemyCtrl.transform.position, _enemyCtrl.EnemyTarget.position);

        if (dist < _enemyCtrl.EnemyFindPlayerDist)
        {
            _enemyCtrl.EnemyMoveTarget();
        }
    }
}
