using System;

public interface IPlayerState
{
    void OnEnter(Action OnCompleted = null);
    void OnUpdate();
    void FixedUpdate();

    void AnimationUpdate();

    void OnExit(Action OnCompleted = null);
    void OnCheckTransition();
    
}
