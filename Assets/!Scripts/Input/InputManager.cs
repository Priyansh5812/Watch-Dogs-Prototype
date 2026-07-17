using UnityEngine;

public class InputManager : ITickableGameModule
{

    Vector3 _axisInput;
    Vector2 _mouseInput;

    public void Initialize()
    {
        
    }

    public void OnModuleReady()
    {
        
    }

    public void Tick(float deltaTime)
    {
        _axisInput = new Vector3(Input.GetAxisRaw("Horizontal") , 0.0f, Input.GetAxisRaw("Vertical"));
        _mouseInput = new Vector2(Input.GetAxis("Mouse X") , -Input.GetAxis("Mouse Y"));
    }

    public Vector3 GetInput() => _axisInput;
    public Vector2 GetMouseInput() => _mouseInput;

    public void DeInitialize()
    {
        
    }

    public void Dispose()
    {
        
    }
}
