using UnityEngine;
using System.Buffers;
using System.Net;
using System;
public static class AreaHelper
{

    const float surfaceAreaFollowDetectionInr = 0.01f;
    const int queryBufferSize = 5;
        
    public static void QueryFollowingSurfaceArea(Vector3 firstRaycastPoint , Vector3 FollowupDirection, Vector3 RaycastDirection, LayerMask SurfaceMask, out Vector3 startPoint , out Vector3 endPoint)
    {
        Vector3 firstPoint = Vector3.zero;
        Vector3 lastPoint = Vector3.zero;
        
        if(FollowupDirection.sqrMagnitude != 1.0f)
            FollowupDirection.Normalize();
        


        RaycastHit[] buffer = ArrayPool<RaycastHit>.Shared.Rent(queryBufferSize);
        int count;
        int i = 0;

        try
        {
            while(true)
            {
                count = Physics.RaycastNonAlloc(firstRaycastPoint + FollowupDirection * surfaceAreaFollowDetectionInr * i,
                RaycastDirection,
                buffer,
                Mathf.Infinity,
                SurfaceMask,
                QueryTriggerInteraction.Ignore);

                if(count > 0)
                {   
                    float minDistance = float.MaxValue;

                    for(int c = 0; c < count; c++)
                    {
                        if(minDistance > buffer[c].distance)
                        {
                            minDistance = buffer[c].distance;
                            lastPoint = buffer[c].point;       
                        }
                    }

                    if(firstPoint == Vector3.zero)
                    {
                        firstPoint = lastPoint;
                    }

                    i++;
                }
                else
                {   
                    break;
                }

            }
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
        finally
        {   
            ArrayPool<RaycastHit>.Shared.Return(buffer);
            startPoint = firstPoint;
            endPoint = lastPoint;
        }

    }



}
