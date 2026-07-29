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
    AnchorPointsOverride anchorOverride;
    (int, int) indices;
    bool performHeightAdjustment;
    public Anim_TargetLinear(PlayerStateDriver driver, Vector3[] points, AnimatorStateInfo info, (int, int) indices, bool performHeightAdjustment, Ease ease, AnchorPointsOverride anchorOverride)
    {
        this.driver = driver;
        this.points = points;
        this.duration = info.length;
        this.indices = indices;
        this.speed = (points[indices.Item1] - points[indices.Item2]).magnitude / (duration / info.speed);
        this.anchorOverride = anchorOverride;
        this.performHeightAdjustment = performHeightAdjustment;
        Debug.Log(speed);
        this.ease = ease;
    }

    public void Refresh()
    {
        t = 0;
        PerformAnchorPointOverride();
        PerformHeightAdjustment();
        driver.StartCoroutine(DebugRoutine());
        //Time.timeScale = 0.25f;
    }

    void PerformHeightAdjustment()
    {
        if (!performHeightAdjustment)
            return;

        float desiredHeight = Mathf.Abs(points[indices.Item2].y - points[indices.Item1].y);
        float ratio = desiredHeight / authoredHeight;
        Debug.Log("Desired Height " + desiredHeight);
        Debug.Log("Ratio: " + ratio);
        float YOffset = points[indices.Item2].y * ratio;
        float diff = YOffset - points[indices.Item2].y;
        targetPoint.y += diff;
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
        fromPoint = points[indices.Item1];
        targetPoint = points[indices.Item2];

        switch (anchorOverride)
        {
            case AnchorPointsOverride.NONE:
            default:
                break;
            case AnchorPointsOverride.LAST_Y:
                fromPoint.y = targetPoint.y = driver.transform.position.y;
                break;
        }
        
    }

}
