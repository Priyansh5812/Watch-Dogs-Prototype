using System;
using UnityEngine;
public class JogState : IPlayerState
{   
    PlayerStateDriver driver;
    InputManager _inputManager;

    public JogState(PlayerStateDriver driver , InputManager inputManager)
    {
        this.driver = driver; 
        _inputManager = inputManager;
    }

    public void OnEnter(Action OnCompleted = null)
    {   
        Debug.Log("Entered Jog State");
        OnCompleted?.Invoke();
    }

    public void OnUpdate()
    {   
        driver.VaultCheckPass();
    }

    public void FixedUpdate()
    {
          if(_inputManager.GetInput().sqrMagnitude > 0.001f)
        {
            //Vector3 newVel = driver.LastVelocity + _inputManager.GetInput() * driver.Data.AccelarationToJog * Time.fixedDeltaTime;
            //driver.CurrentVelocity = Vector3.RotateTowards(driver.CurrentVelocity,newVel,driver.Data.AccelarationToJog , driver.Data.AccelarationToJog);
            driver.CurrentVelocity = driver.LastVelocity + _inputManager.GetInput() * driver.Data.AccelarationToJog * Time.fixedDeltaTime;
            driver.CurrentVelocity = Vector3.ClampMagnitude(driver.CurrentVelocity , driver.Data.MaxJogSpeed);
            
        }
        else
        {   
            driver.CurrentVelocity -= driver.CurrentVelocity * driver.Data.DeacclarationForJog * Time.fixedDeltaTime;
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
        if(driver.CurrentVelocity.sqrMagnitude <= ((driver.Data.MaxWalkSpeed) * (driver.Data.MaxWalkSpeed)) && _inputManager.GetInput().sqrMagnitude <= 0.01f)
        {   
            Debug.LogWarning($"JOG ==> {driver.CurrentVelocity.sqrMagnitude} : {((driver.Data.MaxWalkSpeed) * (driver.Data.MaxWalkSpeed))}");
            driver.InitiateStateChange(typeof(WalkState));
        }

        else if(driver.CurrentVelocity.sqrMagnitude >= (driver.Data.MaxJogSpeed * driver.Data.MaxJogSpeed) && Input.GetKey(KeyCode.LeftShift))
        {
           driver.InitiateStateChange(typeof(RunState));
        }
    }
}
