using System;
using UnityEngine;
public class IdleState : IPlayerState
{       

    PlayerStateDriver driver;
    InputManager _inputManager;

    public IdleState(PlayerStateDriver driver , InputManager inputManager)
    {
        this.driver = driver; 
        _inputManager = inputManager;
    }

    public void OnEnter(Action OnCompleted = null)
    {   
        Debug.Log("Entered Idle State");
        OnCompleted?.Invoke();
    }

    public void OnUpdate()
    {
        
    }

    public void FixedUpdate()
    {   
        if(driver.CurrentVelocity.sqrMagnitude > 0.001f)
        {
            driver.CurrentVelocity -= driver.CurrentVelocity * driver.Data.DeacclarationForWalk * Time.fixedDeltaTime;
        }
        else
        {
            driver.CurrentVelocity = Vector3.zero;
        }
    }

    public void AnimationUpdate()
    {
        driver.Animator.SetFloat("Loco" , driver.CurrentVelocity.magnitude / driver.Data.MaxRunSpeed);
    }

    public async void OnExit(Action OnCompleted = null)
    {   
        Debug.Log("Exited Idle State");
        OnCompleted?.Invoke();
    }

    public void OnCheckTransition()
    {   
        if(_inputManager.GetInput().sqrMagnitude > 0.001f)
        {
            driver.InitiateStateChange(typeof(WalkState));
        }
            
    }



}
