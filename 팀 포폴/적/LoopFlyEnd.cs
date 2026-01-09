using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopFlyEnd : EnemyState
{
    private bool _LoopFlyEnd;
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        _enemyCtrl.LoopFlyEnd();
        _LoopFlyEnd = true;
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       if(_LoopFlyEnd&&stateInfo.normalizedTime>=0.99f)
        {
            _LoopFlyEnd = false;
            animator.SetTrigger("Move");
        }
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }
}
