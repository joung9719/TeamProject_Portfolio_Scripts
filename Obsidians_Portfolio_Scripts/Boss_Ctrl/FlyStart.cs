using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFly : EnemyState
{
     private bool _BossFly = false;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (!_BossFly)
        {
            _enemyCtrl.BossFlyStart();
            _BossFly = true;
        }
        
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_BossFly && stateInfo.normalizedTime>=0.99f)
        {
            _BossFly = false;
            animator.SetTrigger("FlyEnd");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _BossFly = false;
    }
}
