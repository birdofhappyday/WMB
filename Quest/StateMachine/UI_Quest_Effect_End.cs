using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Quest_Effect_End : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        Managers.ResourceMgr.Release(animator.gameObject);
    }
}
