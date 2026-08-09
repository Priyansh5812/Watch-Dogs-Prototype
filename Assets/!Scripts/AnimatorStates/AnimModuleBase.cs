using System;
using UnityEngine;

public interface AnimModuleBase : IDisposable
{   
    void Refresh();
    void Process();
}
