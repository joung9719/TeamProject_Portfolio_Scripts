using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BreathStart : EnemyState
{

    private bool _SkillStart = false;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (!_SkillStart)
        {
            _SkillStart = true;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_SkillStart && stateInfo.normalizedTime>=0.99f)
        {
            _SkillStart = false;
            animator.SetTrigger("FlyStart");
           
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _SkillStart = false;
    }
}
