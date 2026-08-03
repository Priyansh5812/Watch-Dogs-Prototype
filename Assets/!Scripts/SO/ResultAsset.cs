using UnityEngine;

[CreateAssetMenu(fileName = "ResultAsset", menuName = "Scriptable Objects/New Result Asset")]
public class ResultAsset : DecisionAsset<ResultAsset, VaultRequestParams>
{
    [SerializeField] public string targetValue;
    [SerializeField] public WarpAsset warpAsset;
    public override ResultAsset Run(ref VaultRequestParams data) => this;
}
