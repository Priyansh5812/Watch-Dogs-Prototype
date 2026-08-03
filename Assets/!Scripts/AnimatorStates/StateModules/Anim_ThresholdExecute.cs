using UnityEngine;
using System;

public class Anim_ThresholdExecute : AnimModuleBase
{   
    
    float threshold;
    float duration;
    float t_duration;
    AnimModuleBase module;
    bool isExecuted = false;
    public Anim_ThresholdExecute(float duration , AnimModuleBase module , float threshold)
    {   
        this.duration = duration;
        this.threshold = threshold;
        this.module = module;
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
            module?.Process();
            isExecuted = true;      
        }
    }
}

