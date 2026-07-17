using System;

public interface IGameModule : IDisposable
{
    public void Initialize();
    public void OnModuleReady();

    public void DeInitialize();

    // Dispose must be called upon the destruction
}


public interface ITickableGameModule : IGameModule
{
    public void Tick(float deltaTime);
}