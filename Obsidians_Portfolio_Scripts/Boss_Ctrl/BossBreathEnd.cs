using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBreathEnd : EnemyState
{
    private bool _BossBreathEnd=false;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        _enemyCtrl.BossBreathEnd();
        _BossBreathEnd = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       if(_BossBreathEnd&&stateInfo.normalizedTime>=0.99f)
        {
            _BossBreathEnd = false;
            _enemyCtrl.BossSkillEnd();
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }
}
