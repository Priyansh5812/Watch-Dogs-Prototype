using UnityEngine;
using System.Buffers;
using System;
public class AnimModuleRuntime : StateMachineBehaviour , IDisposable
{
    PlayerStateDriver driver;
    Animator animator;

    [SerializeField] AnimModuleConstructor[] stateEnter;
    [SerializeField] AnimModuleConstructor[] stateUpdate;
    [SerializeField] AnimModuleConstructor[] stateExit;

    AnimModuleBase[] entryExecutions;
    AnimModuleBase[] updateExecutions;
    AnimModuleBase[] exitExecutions;

    bool isInitialized = false;

    void TryInitialization(Animator animator, AnimatorStateInfo stateInfo)
    {
        if (isInitialized)
            return;

        try
        {
            this.animator = animator;
            RefreshDriverModule();

            entryExecutions = ArrayPool<AnimModuleBase>.Shared.Rent(stateEnter.Length);
            updateExecutions = ArrayPool<AnimModuleBase>.Shared.Rent(stateUpdate.Length);
            exitExecutions = ArrayPool<AnimModuleBase>.Shared.Rent(stateExit.Length);

            for (int i = 0; i < stateEnter.Length; i++)
            {
                entryExecutions[i] = stateEnter[i].ConstructModule(animator, stateInfo, driver);
            }
            for (int i = 0; i < stateUpdate.Length; i++)
            {
                updateExecutions[i] = stateUpdate[i].ConstructModule(animator, stateInfo, driver);
            }
            for (int i = 0; i < stateExit.Length; i++)
            {
                exitExecutions[i] = stateExit[i].ConstructModule(animator, stateInfo, driver);
            }

            isInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Dispose();
        }
        
    }

    void RefreshDriverModule()
    {
        if (driver == null)
        {
            driver = animator.gameObject.transform.parent.GetComponent<PlayerStateDriver>();
        }
    }

    void RefreshRuntime()
    {
        foreach (var i in entryExecutions)
        {
            i?.Refresh();
        }

        foreach (var i in updateExecutions)
        {
            i?.Refresh();
        }

        foreach (var i in exitExecutions)
        {
            i?.Refresh();
        }
    }
        

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        TryInitialization(animator , stateInfo);

        RefreshRuntime();
        foreach (var i in entryExecutions)
        {
            i?.Process();
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (var i in updateExecutions)
        {
            i?.Process();
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        foreach (var i in exitExecutions)
        {
            i?.Process();
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}

    public void Dispose()
    {
        if (entryExecutions != null)
        {
            ArrayPool<AnimModuleBase>.Shared.Return(entryExecutions);
            entryExecutions = null;
        }

        if (updateExecutions != null)
        {
            ArrayPool<AnimModuleBase>.Shared.Return(updateExecutions);
            updateExecutions = null;
        }

        if (exitExecutions != null)
        {
            ArrayPool<AnimModuleBase>.Shared.Return(exitExecutions);
            exitExecutions = null;
        }

        Debug.Log("Disposed");
       
    }

    void OnDisable()
    {
        Dispose();
    }


    public void DisableRootMotionAndReparent()
    {
        animator.gameObject.transform.SetParent(driver.transform);
        driver.IsUnderRootRotation = false;
        animator.applyRootMotion = false;
    }
}
