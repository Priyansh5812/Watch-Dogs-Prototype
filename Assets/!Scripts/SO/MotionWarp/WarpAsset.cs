using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

[CreateAssetMenu(fileName = "WarpAsset", menuName = "Scriptable Objects/WarpAsset")]
public class WarpAsset : ScriptableObject
{
    public AnimationClip TargetClip;
    public GameObject PreviewPrefab;
    [Range(0f, 1f)]
    public float Start;

    [Range(0f, 1f)]
    public float End = 1f;
    public float PreviewTime;
    public TransformAnimationData Pos_WarpScaleCurves;
    public TransformData[] BakedData;

    [ContextMenu("DebugMax")]
    public void DebugMax() => Debug.Log(GetMaxOffset(Axis.Y));

    public float GetMaxOffset(Axis axis)
    {
        float max = 0;

        foreach(var i in BakedData)
        {   
            float compare;
            switch(axis)
            {
                case Axis.X:
                compare = i.Position.x;
                break;

                case Axis.Y:
                compare = i.Position.y;
                break;

                case Axis.Z:
                default:
                compare = i.Position.z;
                break;
            }

            max = Mathf.Max(compare , max);
        }

        return max;
    }

    public float EvaulateWarpScaleCurve(float t , Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return (Pos_WarpScaleCurves.IsXCurveActive) ? Pos_WarpScaleCurves.XCurve.Evaluate(t) : 0f;

            case Axis.Y:
                return (Pos_WarpScaleCurves.IsYCurveActive) ? Pos_WarpScaleCurves.YCurve.Evaluate(t) : 0f;

            case Axis.Z:
            default:
                return (Pos_WarpScaleCurves.IsZCurveActive) ? Pos_WarpScaleCurves.ZCurve.Evaluate(t) : 0f;
        }
    }

    public float GetStartEventTime() => TargetClip.length * Start; 
    public float GetEndEventTime() => TargetClip.length * End; 

}

[System.Serializable]
public struct TransformAnimationData
{   
    public bool IsXCurveActive;
    public AnimationCurve XCurve;
    [Space(5)]
    public bool IsYCurveActive;
    public AnimationCurve YCurve;
    [Space(5)]
    public bool IsZCurveActive;
    public AnimationCurve ZCurve;
    [Space(5)]
    public Vector3 AuthoredAxisScales;
}

[System.Serializable]
public struct TransformData
{
    public Vector3 Position;
    public Quaternion Rotation;
}

