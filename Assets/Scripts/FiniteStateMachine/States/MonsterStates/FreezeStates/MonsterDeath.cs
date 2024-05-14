using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDeath : MonsterState
{
    private GameObject tempPlayerWeaponGameObject;
    private Weapon tempPlayerWeapon;

    private float _timer;
    private bool isChangeWeapon;
    private bool isRecyclingMonster;
    private bool isRecyclingMonsterHp;
    public MonsterDeath(StateMachine stateMachine) : base(E_State.Death, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        
        if (_monsterStateMachine.isMainCameraFollowing)
        {
            CameraController.Instance.ResetCameraToFollowPlayer();
        }

        _timer = 0f;
        isChangeWeapon = false;
        isRecyclingMonster = false;
        isRecyclingMonsterHp = false;
        
        _monsterStateMachine.monsterWeapon.isHit = true;
        _monsterStateMachine.monsterRigidbody.useGravity = false;
        _monsterStateMachine.monsterCollider.enabled = false;
        _monsterStateMachine.monsterRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToDeath"]);
    }

    public override void UpdatePhysic()
    {
        base.UpdatePhysic();
        _timer += Time.fixedDeltaTime;
        if (!isChangeWeapon && _timer >= 1.1f)
        {
            isChangeWeapon = true;
            RecyclingMonsterWeaponAndCratePlayerWeapon();
        }

        if (!isRecyclingMonsterHp && _timer >= 6f)
        {
            isRecyclingMonsterHp = true;
            _monsterStateMachine.monsterCharacter.monsterHpController.monsterCharacter = null;
            ObjectPoolManager.Instance.RecyclingObject(E_ObjectType.MonsterHp,
                _monsterStateMachine.monsterCharacter.monsterHpController.gameObject);
            _monsterStateMachine.monsterCharacter.monsterHpController = null;
        }

        if (!isRecyclingMonster && _timer >= 10f)
        {
            isRecyclingMonster = true;
            _monsterStateMachine.RecyclingSelf();
        }
    }

    private void RecyclingMonsterWeaponAndCratePlayerWeapon()
    {
        tempPlayerWeaponGameObject = ObjectPoolManager.Instance.GetObject(E_ObjectType.PlayerWeapon);
        
        tempPlayerWeaponGameObject.SetActive(false);
        tempPlayerWeapon = tempPlayerWeaponGameObject.gameObject.GetComponent<Weapon>();
        tempPlayerWeapon.isRandomSetWeapon = false;
        tempPlayerWeapon.weaponModelIndex = _monsterStateMachine.monsterWeapon.nowWeaponIndex;
        tempPlayerWeapon.weaponType = _monsterStateMachine.monsterWeapon.nowWeaponType;
        tempPlayerWeaponGameObject.transform.rotation = _monsterStateMachine.monsterWeapon.transform.rotation;
        tempPlayerWeapon.Init();
        tempPlayerWeapon.DiscardItem(_monsterStateMachine.monsterWeapon.transform.position +
                                     Vector3.up * 10f);
        tempPlayerWeaponGameObject.SetActive(true);
        
        ObjectPoolManager.Instance.RecyclingObject(E_ObjectType.MonsterWeapon,
            _monsterStateMachine.monsterWeapon.gameObject);
    }
}
