using UnityEditor.Rendering;
using UnityEngine;

public class Anim_Walk180 : StateMachineBehaviour
{
    PlayerStateDriver driver;
    float duration , t_duration;
    bool isSetuped = false;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        RefreshDriverModule(animator);
        animator.gameObject.transform.SetParent(null);
        driver.IsUnderRootRotation = true;
        animator.applyRootMotion = true;
        isSetuped = false;
        duration = t_duration = stateInfo.length;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        driver.transform.position = animator.gameObject.transform.position;
        t_duration -= Time.deltaTime;
        t_duration = Mathf.Clamp(t_duration , 0 , duration);
        if((t_duration / duration) < 0.375f && !isSetuped)
        {
            animator.gameObject.transform.SetParent(driver.transform);
            driver.IsUnderRootRotation = false;
            animator.applyRootMotion = false;
            isSetuped = true;
        }
    }


    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        RefreshDriverModule(animator);

    }

    void RefreshDriverModule(Animator animator)
    {
        if(driver == null)
        {
            driver = animator.gameObject.transform.parent.GetComponent<PlayerStateDriver>();
        }
    }

}   
