using UnityEngine;
using AYellowpaper.SerializedCollections;
[CreateAssetMenu(fileName = "AnimationTriggerData", menuName = "Scriptable Objects/AnimationTriggerStorage")]
public class VaultTriggerStorage : ScriptableObject
{   
    [SerializedDictionary("Count","Triggers")]
    [SerializeField] SerializedDictionary<int , VaultTriggerData> animationTriggerNames;

    public bool DoesHitCountExist(int key) => this.animationTriggerNames.ContainsKey(key);
    public VaultTriggerData GetTriggerData(int key)
    {
        if(!DoesHitCountExist(key))
            return default;

        return this.animationTriggerNames[key];
    } 
}


[System.Serializable]
public struct TriggerInfo
{
    public string triggerName;
    public float minTriggerAnimationDistance;
    public float maxTriggerAnimationDistance;
    public bool useLastSpeed;
    public float maxPostTriggerSpeed;
    [Min(0f)] public float postTriggerSpeed;

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
    public TriggerInfo trigger;
    public System.Type lastStateType;
    public Vector3 startPoint;
    public Vector3 endPoint;
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