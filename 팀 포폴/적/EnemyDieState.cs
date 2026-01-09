using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDieState : EnemyState
{
   

    // OnStateEnter는 Animator에서 _enemyCtrl 할당
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       base.OnStateEnter(animator,stateInfo,layerIndex);
    }

    // OnStateUpdate에서 안전하게 null 체크 후 실행
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_enemyCtrl == null)
        return;
        if(_enemyCtrl.EnemyIsDie)
        return;

        if(_enemyCtrl.isBoss||_enemyCtrl.BossHP<=0)
        {
            _enemyCtrl.BossDie();
        }
        else if(_enemyCtrl.isNamedBoss&&_enemyCtrl.NamedBosshp<=0)
        {
            _enemyCtrl.NamedBossDie();
        }
        else if(_enemyCtrl.isEnemy&&_enemyCtrl.EnemyHp<=0)
        {
            _enemyCtrl.EnemyDie();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 필요시 추가
    }
}
