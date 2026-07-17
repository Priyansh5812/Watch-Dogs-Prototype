using System;
using UnityEngine;
public class WalkState : IPlayerState 
{   

    PlayerStateDriver driver;
    InputManager _inputManager;
    public WalkState(PlayerStateDriver driver , InputManager inputManager)
    {
        this.driver = driver; 
        _inputManager = inputManager;
    }

    public void OnEnter(Action OnCompleted = null)
    {   
        Debug.Log("Entered Walk State");
        OnCompleted?.Invoke();
    }

    public void OnUpdate()
    {
    }

    public void FixedUpdate()
    {   
         if(_inputManager.GetInput().sqrMagnitude > 0.001f)
        {
            driver.CurrentVelocity = driver.LastVelocity + _inputManager.GetInput() * driver.Data.AccelarationToWalk * Time.fixedDeltaTime;
            driver.CurrentVelocity = Vector3.ClampMagnitude(driver.CurrentVelocity , driver.Data.MaxWalkSpeed);
        }
        else
        {
            driver.CurrentVelocity -= driver.CurrentVelocity * driver.Data.Deacclaration* Time.fixedDeltaTime;
        }
    }

    public void AnimationUpdate()
    {
        driver.Animator.SetFloat("Loco" , driver.CurrentVelocity.magnitude / driver.Data.MaxRunSpeed);
    }

    public void OnExit(Action OnCompleted = null)
    {
        OnCompleted?.Invoke();
    }

    public void OnCheckTransition()
    {
        if(driver.CurrentVelocity.sqrMagnitude <= (driver.Data.MaxWalkSpeed/2) * (driver.Data.MaxWalkSpeed/2) && _inputManager.GetInput().sqrMagnitude <= 0.01f)
        {
            driver.InitiateStateChange(typeof(IdleState));
        }
    }



}
