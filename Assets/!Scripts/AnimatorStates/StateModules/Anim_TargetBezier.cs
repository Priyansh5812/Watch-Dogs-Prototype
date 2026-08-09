using UnityEngine;
using System.Collections;
using PrimeTween;
public class Anim_TargetBezier : AnimModuleBase
{
    PlayerStateDriver driver;
    Vector3[] points;
    float duration;
    float speed;
    float t1,t2,t;
    Ease ease;
    Vector3 anchorPoint;
    public Anim_TargetBezier(PlayerStateDriver driver, Vector3[] points, AnimatorStateInfo info, float additionalSpeedMultiplier, Ease ease, AnchorPointType anchorType)
    {
        this.driver = driver;
        this.points = points;
        this.duration = info.length;
        this.speed = (points[0] - points[1]).magnitude / (duration / info.speed);
        this.speed*= additionalSpeedMultiplier;
        Debug.Log(speed);
        this.ease = ease;
        CalculateAnchorPoint(anchorType);
    }

    public void Refresh()
    {   
        t1 = t2 = t = 0;
        driver.StartCoroutine(DebugRoutine());
        
    }

    public void Process()
    {
        if (t >= 1.0f)
        {
            Time.timeScale = 1f;
            return;
        }
        //Time.timeScale = 0.25f;
        Vector3 p1 = ProcessInterpolation(points[0], anchorPoint, ref t1);
        Vector3 p2 = ProcessInterpolation(anchorPoint, points[1], ref t2);
        Vector3 pRes = ProcessInterpolation(p1, p2, ref t);

        driver.transform.position = pRes;
    }

    IEnumerator DebugRoutine()
    {
        while (true)
        {   
            Debug.DrawLine(points[0], points[1], Color.red);

            yield return null;
        }
    }

    

    Vector3 ProcessInterpolation(Vector3 pointA, Vector3 pointB, ref float t)
    {
        Vector3 res = Vector3.Lerp(pointA, pointB,Easing.Evaluate(t, ease));
        t += Time.deltaTime * speed;
        t = Mathf.Clamp01(t);
        return res;
    }


    void CalculateAnchorPoint(AnchorPointType anchorType)
    {
        switch (anchorType)
        {
            case AnchorPointType.UP_FORWARD:
                anchorPoint = new Vector3(points[0].x, points[1].y, points[0].z); // Points going from down to up
                break;

            case AnchorPointType.DOWN_FORWARD:
                anchorPoint = new Vector3(points[1].x, points[0].y, points[1].z); // Points going from up to down
                break;

            default:
                anchorPoint = (points[0] + points[1]) / 2; // Simple Midpoint
                break;
        }
    }

    public void Dispose()
    {
    }
}




public enum AnchorPointType
{ 
    NONE,
    UP_FORWARD,
    DOWN_FORWARD
}

public enum AnchorPointsOverride
{ 
    NONE,
    LAST_Y
}
