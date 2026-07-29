using UnityEngine;
using System.Collections.Generic;
public abstract class DecisionAsset<T1 , T2> : ScriptableObject where T2 : struct
{   
    HashSet<DecisionAsset<T1 , T2>> setStack = new();
    public abstract T1 Run(ref T2 data);    

    protected void AddAssetToTrail(DecisionAsset<T1,T2> asset)
    {
        setStack ??= new();
        if(!IsAssetFollowedUp(asset))
        {   try
            {
                setStack.Add(asset);
            }
            catch
            {
                throw new System.Exception($"Asset trail is compromised, Duplicated Asset : {asset.name}");
            }
        }

        
    }

    protected void RemoveAssetFromTrail(DecisionAsset<T1,T2> asset)
    {
        setStack ??= new();
        if(IsAssetFollowedUp(asset))
        {
           try
            {
                setStack.Remove(asset);
            }
            catch
            {
                throw new System.Exception($"Asset trail is compromised, Unknown Asset : {asset.name}");
            }
        }
    }

    protected bool IsAssetFollowedUp(DecisionAsset<T1 , T2> asset) => setStack == null ? throw new System.Exception("SetStack is null") : setStack.Contains(asset);
}


public struct VaultRequestParams
{
    public int ObstacleRayHitCount;
    public float ObstacleProximity;
    public float ObstacleWidth;
    public float ObstacleHeight;
    // TODO : based on speed -> VVVVV IMP
}


[System.Serializable]
public struct TriggerInfo
{
    public string triggerName;
    [Header("Distance Band for animation to happen")]
    [Min(0f)] public float minTriggerAnimationDistance;
    [Min(0f)] public float maxTriggerAnimationDistance;

    [Header("Width Band for animation to happen")]
    [Min(0f)] public float minObstacleWidth;
    [Min(0f)] public float maxObstacleWidth;
    
    [Header("Height Band for animation to happen")]
    [Min(0f)] public float minObstacleHeight;
    [Min(0f)] public float maxObstacleHeight;
}

[System.Serializable]
public struct RaycastInfo
{
    [Range(0f , 1f)] public float t;
    [Min(0f)] public float distance;
}

[System.Serializable]
public struct VaultContext
{
    public string trigger;
    public System.Type lastStateType;
    public Vector3[] traversalPoints;
}

[System.Serializable]
public class VaultTriggerData
{   
    public TriggerInfo[] possibleTriggers;
    int randomIndex;

    public VaultTriggerData(TriggerInfo[] a = null, int b = 0)
    {   
        possibleTriggers = a;
        randomIndex = b;
        UpdateRandomIndex();
    }

    public TriggerInfo GetRandomTrigger()
    {   
        if(possibleTriggers == null || possibleTriggers.Length == 0)
        {
            Debug.LogError("Trigger Array is null");
            return default;
        }
        return possibleTriggers[randomIndex];
    }

    public void UpdateRandomIndex() => randomIndex = UnityEngine.Random.Range(0 , possibleTriggers.Length);
}