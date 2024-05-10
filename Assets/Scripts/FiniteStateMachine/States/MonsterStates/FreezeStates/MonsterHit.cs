using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHit : MonsterState
{
    public Vector3 HitPosition;
    
    // 到被击中点的方向
    private Vector3 _directionToHit;
    // 计时器
    private float _timer;
    
    public MonsterHit(StateMachine stateMachine) : base(E_State.Hit, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        _timer = 0f;
        CalculateHitDirectionAndToHit();
    }

    public override void UpdatePhysic()
    {
        _timer += Time.deltaTime;
        if (_timer >= 0.4f)
        {
            _monsterStateMachine.ChangeState(_monsterStateMachine.IdleState);
        }
    }
    // 计算击中方向设置播放击中动画
    private void CalculateHitDirectionAndToHit()
    {
        _directionToHit = (HitPosition - _monsterStateMachine.transform.position).normalized;
        // 点乘用来表示两个向量间的夹角，向量B在向量A上的投影长度，通过投影长度正负判断前后方位再通过角度判断左右
        // 在前方
        if (Vector3.Dot(_directionToHit, _monsterStateMachine.transform.forward) >= 0f)
        {
            if (Vector3.Angle(_directionToHit, _monsterStateMachine.transform.forward) <= 45f)
            {
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToHitBack"]);
            }
            else if(Vector3.Dot(_directionToHit, _monsterStateMachine.transform.right) >= 0f)
            {
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToHitRight"]);
            }
            else
            {
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToHitLeft"]);
            }
        }
        // 后方
        else
        {
            if (Vector3.Angle(_directionToHit, -_monsterStateMachine.transform.forward) <= 45f)
            {
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToHitFront"]);
            }
            else if(Vector3.Dot(_directionToHit, _monsterStateMachine.transform.right) >= 0f)
            {
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToHitRight"]);
            }
            else
            {
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToHitLeft"]);
            }
        }
    }
}
