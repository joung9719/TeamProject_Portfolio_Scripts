using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBreath : EnemyState
{
    private bool _BossBreath = false;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (!_BossBreath)
        {
            _enemyCtrl.BossBreath();
            _BossBreath = true;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_BossBreath&&_enemyCtrl.BossSkillAnimEnd("Breath"))
        {
            _BossBreath = false;
            animator.SetTrigger("BreathEnd");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _BossBreath = false;
    }
}
