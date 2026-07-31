using UnityEditor;
using UnityEngine;

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
}
