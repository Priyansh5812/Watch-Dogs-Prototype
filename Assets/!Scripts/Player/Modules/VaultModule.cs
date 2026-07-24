using System;
using UnityEngine;
using System.Collections.Generic;
using System.Buffers;

public class VaultModule : IDisposable
{
    PlayerStateDriver driver;
    VaultTriggerStorage data;
    TriggerInfo[] possibleTriggers;
    List<RaycastInfo> raycastPoints; 
    RaycastHit[] buffer;

    VaultContext vContext;

    int randomIndex;



    public VaultModule(PlayerStateDriver driver , VaultTriggerStorage data)
    {
        this.driver = driver;
        this.data = data;
        this.raycastPoints = driver.raycastPoints;
        buffer = ArrayPool<RaycastHit>.Shared.Rent(5);

    }   

    public void VaultCheckPass()
    {
        Vector3 startPoint = driver.transform.position;
        int c = 0;
        float minPointDistance = float.MaxValue;
        foreach(var i in raycastPoints)
        {
            Vector3 point = startPoint + Vector3.up * Mathf.Lerp(0 , driver.cc.height , i.t);
            Array.Clear(buffer, 0, buffer.Length);
            int count = Physics.RaycastNonAlloc(point,driver.artTransform.forward,buffer,i.distance,driver.targetVaultLayer,QueryTriggerInteraction.Ignore);
            if(count > 0)
            {
                c++;
                for(int j = 0; j< count; j++)
                {
                    minPointDistance = Mathf.Min(minPointDistance , buffer[j].distance);
                }
            }
        }

        if(data.DoesHitCountExist(c))
        {   
            VaultTriggerData data = this.data.GetTriggerData(c);
            TriggerInfo triggerInfo = data.GetRandomTrigger();

            // Yes I am in range of an obstacle where I can perform the animation
            if(minPointDistance >= triggerInfo.minTriggerAnimationDistance && minPointDistance <= triggerInfo.maxTriggerAnimationDistance)
            {   
                
                Debug.LogWarning("Min Distance : "+minPointDistance);
                vContext = new VaultContext(){trigger = triggerInfo};
                switch(driver.GetCurrentState())
                {
                    case RunState:
                        vContext.lastStateType = typeof(RunState);
                        break;
                    case JogState:
                        vContext.lastStateType = typeof(JogState);
                        break;
                    default:
                        break;
                }
                data.UpdateRandomIndex();
                driver.InitiateStateChange(typeof(VaultState));
            }
        }
    }

    public VaultContext GetVaultContext() => this.vContext;
    
    void UpdateRandomIndex() => randomIndex = UnityEngine.Random.Range(0 , possibleTriggers.Length);

    public void Dispose()
    {
        if(buffer != null)
        {
            ArrayPool<RaycastHit>.Shared.Return(buffer);
            buffer = null;
        }
    }
}
