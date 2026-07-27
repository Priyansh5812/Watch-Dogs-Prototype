using System;
using UnityEngine;
using System.Collections.Generic;
using System.Buffers;

public class VaultModule : IDisposable
{
    PlayerStateDriver driver;
    VaultTriggerStorage data;
    List<RaycastInfo> raycastPoints; 
    RaycastHit[] buffer;
    VaultContext vContext;
    Vector3[] traversalPoints;

    public VaultModule(PlayerStateDriver driver , VaultTriggerStorage data)
    {
        this.driver = driver;
        this.data = data;
        this.raycastPoints = driver.raycastPoints;
        buffer = ArrayPool<RaycastHit>.Shared.Rent(5);
        traversalPoints = ArrayPool<Vector3>.Shared.Rent(3);
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
                float surfaceArea = DetermineObstacleSurfaceToCover(minPointDistance);
                
                vContext = new VaultContext(){trigger = triggerInfo};
                vContext.traversalPoints = this.traversalPoints;

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

    float DetermineObstacleSurfaceToCover(float minPointDistance)
    {   
        Vector3 firstRaycastPoint = driver.transform.TransformPoint(driver.cc.center) + Vector3.up * driver.cc.height/2;
        firstRaycastPoint += driver.artTransform.forward * (minPointDistance + 0.001f);
        
        AreaHelper.QueryFollowingSurfaceArea(firstRaycastPoint,
        driver.artTransform.forward,
        Vector3.down,
        driver.targetVaultLayer,
        out Vector3 startPoint,
        out Vector3 endPoint);

        Debug.Log($"startPoint:{startPoint} , endPoint {endPoint}");
        GenerateTraversalPoints(startPoint, endPoint);

        return (endPoint - startPoint).magnitude;
    }

    void GenerateTraversalPoints(Vector3 startPoint, Vector3 endPoint)
    {
        traversalPoints[0] = driver.transform.position;
        traversalPoints[1] = startPoint;
        traversalPoints[2] = endPoint;
    }

    public VaultContext GetVaultContext() => this.vContext;

    public void Dispose()
    {
        if(buffer != null)
        {
            ArrayPool<RaycastHit>.Shared.Return(buffer);
            buffer = null;
        }

        if (traversalPoints != null)
        {
            ArrayPool<Vector3>.Shared.Return(traversalPoints);
            traversalPoints = null;
        }
    }
}
