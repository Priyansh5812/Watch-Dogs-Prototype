using UnityEngine;
using UnityEngine.Events;
using System;

public class Anim_ThresholdExecute : AnimModuleBase
{   
    UnityEvent targetAction;
    float threshold;

    float duration;
    float t_duration;

    bool isExecuted = false;
    public Anim_ThresholdExecute(float duration , UnityEvent targetAction, float threshold)
    {   
        this.duration = duration;
        this.targetAction = targetAction;
        this.threshold = threshold;
    }

    public void Refresh()
    {
        isExecuted = false;
        t_duration = duration;
    }

    public void Process()
    {   
       if(isExecuted)
            return;

        t_duration -= Time.deltaTime;
        
        if((t_duration / duration) < threshold)
        {   
            targetAction?.Invoke();
            isExecuted = true;      
        }
    }
}

