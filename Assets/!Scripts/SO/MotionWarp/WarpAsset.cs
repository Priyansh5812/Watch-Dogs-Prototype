using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "WarpAsset", menuName = "Scriptable Objects/WarpAsset")]
public class WarpAsset : ScriptableObject
{
    public AnimationClip TargetClip;
    public GameObject PreviewPrefab;
    //[Range(0f, 1f)]
    //public float Start;
    //[Range(0f, 1f)]
    //public float End = 1f;
    public float PreviewTime;
    public int SampleRate;
    public List<WarpWindow> windows;
    
}

[System.Serializable]
public struct WarpWindow
{
    [Range(0f, 1f), HideInInspector] public float start;
    [Range(0f, 1f), HideInInspector] public float end;
    public Color windowBoundsColor;
    public TransformCurves curves;
    public Vector3 AuthoredValues;
    [Header("Indices for Considering Trajectory Array")]
    [Min(0)] public int Traj_FromIndex;
    [Min(0)] public int Traj_ToIndex;
    public bool IsCurrentTimeUnderBounds(float elapsedTime, float clipLength) => start * clipLength <= elapsedTime && end * clipLength >= elapsedTime;  
}

[System.Serializable]
public struct TransformCurves
{
    public bool isXCurveActive;
    public AnimationCurve XCurve;
    public bool isYCurveActive;
    public AnimationCurve YCurve;
    public bool isZCurveActive;
    public AnimationCurve ZCurve;

}

