using System;
using UnityEngine;

public class RootMotionRuntime : MonoBehaviour
{
    public Animator anim;
    event Action AnimatorRuntimeCallback = null;

    public void AddRuntime(Action Runtime)
    {
        AnimatorRuntimeCallback += Runtime;
    }

    public void RemoveRuntime(Action Runtime)
    {
        AnimatorRuntimeCallback -= Runtime;
    }

    //void OnAnimatorMove()
    //{
    //    //AnimatorRuntimeCallback?.Invoke();
    //    if (!anim.applyRootMotion)
    //        return;

    //    transform.position += anim.deltaPosition;
    //    transform.rotation *= anim.deltaRotation;
    //}

    void OnDisable()
    {
        AnimatorRuntimeCallback = null;
    }
}
