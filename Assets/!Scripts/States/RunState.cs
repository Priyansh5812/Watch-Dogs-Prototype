using System;

public class RunState : IPlayerState
{   

    PlayerStateDriver driver;

    public RunState(PlayerStateDriver driver)
    {
        this.driver = driver; 
    }


    public void OnEnter(Action OnCompleted = null)
    {
        OnCompleted?.Invoke();
    }

    public void OnUpdate()
    {
    }

    public void FixedUpdate()
    {
    }

    public void AnimationUpdate()
    {
        
    }

    public void OnExit(Action OnCompleted = null)
    {
        OnCompleted?.Invoke();
    }

    public void OnCheckTransition()
    {

    }
}
