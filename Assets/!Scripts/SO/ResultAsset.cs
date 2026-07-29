using UnityEngine;

[CreateAssetMenu(fileName = "ResultAsset", menuName = "Scriptable Objects/New Result Asset")]
public class ResultAsset : DecisionAsset<string, VaultRequestParams>
{
    [SerializeField] string targetValue;

    public override string Run(ref VaultRequestParams data) => targetValue;
}
