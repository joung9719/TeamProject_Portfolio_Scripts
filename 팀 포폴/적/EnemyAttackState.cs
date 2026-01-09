using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class EnemyAttackState : EnemyState
{
    private bool attackFinished = false;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_enemyCtrl==null)
        _enemyCtrl=animator.GetComponentInParent<EnemyCtrl>();
        attackFinished = false;
        animator.ResetTrigger("Move");
        animator.ResetTrigger("Idle");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_enemyCtrl.EnemyIsDie || _enemyCtrl.EnemyTarget == null)
            return;

            if(_enemyCtrl.enemyAttacking||_enemyCtrl.SkillUse||_enemyCtrl.BreathUse)
        {
            _enemyCtrl.EnemyAgent.isStopped=true;
            return;
        }

        bool isAttackingAnim=stateInfo.IsName("Attack")||
        stateInfo.IsName("Attack02")||
        stateInfo.IsName("Attack03")||
        stateInfo.IsName("Attack03")||
        stateInfo.IsName("HandAttack");

        if(isAttackingAnim&&stateInfo.normalizedTime<1.0f)
        {
            _enemyCtrl.EnemyAgent.isStopped=true;
            return;
        }
        if(!attackFinished&&stateInfo.normalizedTime>=0.95f)
        {
            attackFinished=true;
            animator.SetTrigger("Move");
        }

        
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Exit 직전에 Move 트리거 한번 더 안전하게 전달
        if (attackFinished)
        {
            animator.SetTrigger("Move");
        }
    }

}
