using System;

public class JogState : IPlayerState
{   
    PlayerStateDriver driver;

    public JogState(PlayerStateDriver driver)
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
