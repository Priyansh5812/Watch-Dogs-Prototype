#if UNITY_EDITOR
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "Decision Asset" , menuName = "Scriptable Objects/New Decision Asset")]
public class ConditionDecisionAsset : DecisionAsset<string , VaultRequestParams>
{   
    [SerializeField] EvaluationType evaluationType;
    [SerializeField] ComparisonTarget comparisonTarget;
    [SerializeField] float comparisonField;
    [SerializeField] float minComparisonField;
    [SerializeField] float maxComparisonField;
    [SerializeField] DecisionAsset<string, VaultRequestParams>[] followupAssets;
    bool Evaluate(ref VaultRequestParams req)
    {
        float targetValue;

        switch (comparisonTarget)
        {
            case ComparisonTarget.HITCOUNT:
                targetValue = req.ObstacleRayHitCount;
                break;

            case ComparisonTarget.DISTANCE:
                targetValue = req.ObstacleProximity;
                break;

            case ComparisonTarget.WIDTH:
                targetValue = req.ObstacleWidth;
                break;

            case ComparisonTarget.HEIGHT:
                targetValue = req.ObstacleHeight;
                break;

            default:
                targetValue = 0;
                break;
            
        }

        // ----------------------

        switch (evaluationType)
        {
            case EvaluationType.EQUAL:
                return IsEqual(targetValue);

            case EvaluationType.RANGE:
                return IsInRange(targetValue);

            case EvaluationType.NONE:
                return true;
        }

        return false;
    }

    bool IsEqual(float value) => Mathf.Abs(value - comparisonField) < 0.001f;
    bool IsInRange(float value) => minComparisonField <= value && maxComparisonField >= value;

    string FollowupDecision(ref VaultRequestParams req)
    {   
        if(followupAssets == null)
            return string.Empty;

        foreach (var asset in followupAssets)
        {
            if (asset == null)
                continue;
            this.AddAssetToTrail(asset);

            string res = asset.Run(ref req);
            
            this.RemoveAssetFromTrail(asset);
            
            if (string.IsNullOrEmpty(res))
                continue;

            return res;
        }

        return string.Empty;
    }


    public override string Run(ref VaultRequestParams req)
    {
        if (Evaluate(ref req))
        {
            return FollowupDecision(ref req);
        }

         return string.Empty;
    }

    private void OnValidate()
    {
        if (minComparisonField > maxComparisonField)
            (minComparisonField, maxComparisonField) =
                (maxComparisonField, minComparisonField);
    }

    public enum EvaluationType
    {   
        NONE,
        EQUAL,
        RANGE,
    }

    public enum ComparisonTarget
    {   
        NONE,
        HITCOUNT,
        DISTANCE,
        WIDTH,
        HEIGHT
    }

}

// EDITOR CLASS---
#if UNITY_EDITOR
[CustomEditor(typeof(ConditionDecisionAsset))]
public class ConditionDecisionAssetEditor : Editor
{
    SerializedProperty evaluationType;
    SerializedProperty comparisonTarget;
    SerializedProperty comparisonField;
    SerializedProperty minComparisonField;
    SerializedProperty maxComparisonField;
    SerializedProperty followupAssets;

    private void OnEnable()
    {
        evaluationType = serializedObject.FindProperty("evaluationType");
        comparisonTarget = serializedObject.FindProperty("comparisonTarget");
        comparisonField = serializedObject.FindProperty("comparisonField");
        minComparisonField = serializedObject.FindProperty("minComparisonField");
        maxComparisonField = serializedObject.FindProperty("maxComparisonField");
        followupAssets = serializedObject.FindProperty("followupAssets");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(evaluationType);
        EditorGUILayout.PropertyField(comparisonTarget);

        EditorGUILayout.Space();

        switch ((ConditionDecisionAsset.EvaluationType)evaluationType.enumValueIndex)
        {
            case ConditionDecisionAsset.EvaluationType.EQUAL:
                EditorGUILayout.PropertyField(comparisonField);
                break;

            case ConditionDecisionAsset.EvaluationType.RANGE:
                EditorGUILayout.PropertyField(minComparisonField);
                EditorGUILayout.PropertyField(maxComparisonField);
                break;
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(followupAssets, true);

        serializedObject.ApplyModifiedProperties();
    }
}

#endif