using System;
using UnityEngine;
public class RunState : IPlayerState
{   

    PlayerStateDriver driver;
    InputManager _inputManager;
    public RunState(PlayerStateDriver driver, InputManager inputManager)
    {
        this.driver = driver; 
        _inputManager = inputManager;
    }


    public void OnEnter(Action OnCompleted = null)
    {   
        OnCompleted?.Invoke();
    }

    public void OnUpdate()
    {
        driver.VaultCheckPass();
    }

    public void FixedUpdate()
    {
        if(_inputManager.GetInput().sqrMagnitude > 0.001f && Input.GetKey(KeyCode.LeftShift))
        {
            //Vector3 newVel = driver.LastVelocity + _inputManager.GetInput() * driver.Data.AccelarationToRun * Time.fixedDeltaTime;
            //driver.CurrentVelocity = Vector3.RotateTowards(driver.CurrentVelocity,newVel,driver.Data.animationTurnLerpSpeed , driver.Data.AccelarationToRun);
            driver.CurrentVelocity = driver.LastVelocity + _inputManager.GetInput() * driver.Data.AccelarationToRun * Time.fixedDeltaTime;
            driver.CurrentVelocity = Vector3.ClampMagnitude(driver.CurrentVelocity , driver.Data.MaxRunSpeed);
        }
        else
        {
            driver.CurrentVelocity -= driver.CurrentVelocity * driver.Data.DeacclarationForRun * Time.fixedDeltaTime;
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
        if(driver.CurrentVelocity.sqrMagnitude <= ((driver.Data.MaxJogSpeed) * (driver.Data.MaxJogSpeed))-1.0f)
        {
            driver.InitiateStateChange(typeof(JogState));
        }

    }
}
