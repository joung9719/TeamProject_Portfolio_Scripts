using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class PlayerAnim : MonoBehaviour
{
    Animator _anim;

    string moveBool = "Move";
    string runBool = "Run";
    string ShootBool = "Shoot";
    string jumpBool = "Jump";
    string DieBool = "Die";
    string throwBool="Throw";

    void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void SetMove(bool isMoving)
    {
        _anim.SetBool(moveBool, isMoving);
    }

    public void SetRun(bool isRunning)
    {
        _anim.SetBool(runBool, isRunning);
    }

    public void SetAttack(bool isAttacking)
    {
        _anim.SetBool(ShootBool, isAttacking); //트리거로 바꾸는거 고려
    }

    public void SetJump(bool isJumpLand)
    {
        _anim.SetBool(jumpBool, isJumpLand);
    }

    public void SetDie(bool isDie)
    {
        _anim.SetBool(DieBool, isDie);
    }

    public void SetThrow(bool isThrow)
    {
        _anim.SetBool(throwBool,isThrow);
    }
}