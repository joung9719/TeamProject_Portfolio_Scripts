using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTailAttack : EnemyState
{
    private bool _TailAttack = false;
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if(!_TailAttack)
        {
            _enemyCtrl.BossTailAttack();
            _TailAttack = true;
        }
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       if(_enemyCtrl.BossHP<=500&&_enemyCtrl.BossSkillAnimEnd("BossWalk"))
        {
            _TailAttack = false;
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("BossTailAttack"))
            {
                animator.SetTrigger("TailAttack");
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _TailAttack = false;
    }
}
