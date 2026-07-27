using UnityEngine;
using System.Collections;
using PrimeTween;

public class Anim_TargetLinear : AnimModuleBase
{
    PlayerStateDriver driver;
    Vector3[] points;
    float duration;
    float speed;
    float authoredHeight = 0.89f;
    float t;
    Vector3 fromPoint, targetPoint;
    Ease ease;
    AnchorPointOverride anchorOverride;

    public Anim_TargetLinear(PlayerStateDriver driver, Vector3[] points, AnimatorStateInfo info, float additionalSpeedMultiplier, Ease ease, AnchorPointOverride anchorOverride)
    {
        this.driver = driver;
        this.points = points;
        this.duration = info.length;
        this.speed = (points[0] - points[1]).magnitude / (duration / info.speed);
        this.speed *= additionalSpeedMultiplier;
        this.anchorOverride = anchorOverride;
        Debug.Log(speed);
        this.ease = ease;
    }

    public void Refresh()
    {
        t = 0;
        PerformAnchorPointOverride();
        float desiredHeight = Mathf.Abs(points[1].y - points[0].y);
        float ratio = desiredHeight / authoredHeight;
        Debug.Log("Desired Height "+desiredHeight);
        Debug.Log("Ratio: " + ratio);
        float YOffset = points[1].y * ratio;
        float diff = YOffset - points[1].y;
        targetPoint.y += diff;
        driver.StartCoroutine(DebugRoutine());
    }



    public void Process()
    {
        if (t >= 1.0f)
        {
            return;
        }

        driver.transform.position = ProcessInterpolation(fromPoint, targetPoint, ref t);

    }

    IEnumerator DebugRoutine()
    {
        while (true)
        {
            Debug.DrawLine(points[0], points[1], Color.cyan);

            yield return null;
        }
    }



    Vector3 ProcessInterpolation(Vector3 pointA, Vector3 pointB, ref float t)
    {
        Vector3 res = Vector3.Lerp(pointA, pointB, Easing.Evaluate(t, ease));
        t += Time.deltaTime * speed;
        t = Mathf.Clamp01(t);
        return res;
    }

    void PerformAnchorPointOverride()
    {
        fromPoint = points[0];
        targetPoint = points[1];

        switch (anchorOverride)
        {
            case AnchorPointOverride.NONE:
            default:
                break;

            case AnchorPointOverride.PROJECT_X:
                targetPoint.x = fromPoint.x;
                break;

            case AnchorPointOverride.PROJECT_Y:
                targetPoint.y = fromPoint.y;
                break;

            case AnchorPointOverride.PROJECT_Z:
                targetPoint.z = fromPoint.z;
                break;
        }
        
    }

}
